using Moq;
using NUnit.Framework;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace TheSubDBSharp.Client.Tests
{
    [TestFixture]
    public class TheSubDBSharpTests
    {
        #region Private Fields

        private static readonly object[] ResponseHeaders =
        {
                new object[] { null },
                new object[] { new List<Parameter>() },
                new object[] { new List<Parameter>() { new Parameter { Name = "Content-Language", Value = null } } },
                new object[] { new List<Parameter>() { new Parameter { Name = "Content-Language", Value = string.Empty } } }
        };

        private Mock<IRestClient> mockRestClient = null;
        private string mockSubtitle = "this is a subtitle";
        private byte[] mockSubtitleBytes = null;
        private Mock<Stream> mockSubtitleStream = null;

        #endregion Private Fields

        #region Public Methods

        [OneTimeSetUp]
        public void Setup()
        {
            mockRestClient = new Mock<IRestClient>();
            mockSubtitleBytes = Encoding.UTF8.GetBytes(mockSubtitle);
            mockSubtitleStream = new Mock<Stream>();
            mockSubtitleStream.SetupGet(x => x.CanRead).Returns(true);
            mockSubtitleStream.SetupGet(x => x.CanSeek).Returns(true);
            mockSubtitleStream.SetupGet(x => x.Length).Returns(mockSubtitleBytes.Length);
        }

        [Test]
        public void TestDownload()
        {
            IRestRequest actualRequest = null;
            var languages = "en,pt,es";
            var expectedHash = "ffd8d4aa68033dc03d1c8ef373b9028c";
            var expectedLanguage = "pt";
            var expectedSubtitleContent = "subtitle content";

            var mockResponse = new Mock<IRestResponse>();
            mockResponse.SetupGet(x => x.StatusCode).Returns(HttpStatusCode.OK);
            mockResponse.SetupGet(x => x.Content).Returns(expectedSubtitleContent);
            mockResponse.SetupGet(x => x.Headers).Returns(new List<Parameter>() { new Parameter { Name = "Content-Language", Value = expectedLanguage } });

            mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>(r => actualRequest = r)
                .Returns(mockResponse.Object);

            var target = new SubDBClient(mockRestClient.Object);
            var actualResult = target.Download(expectedHash, languages);

            this.AssertRequest(actualRequest, Method.GET, "download", expectedHash);
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(expectedLanguage, actualResult.Language);
            Assert.IsFalse(actualResult.NotFound);
            Assert.AreEqual(expectedSubtitleContent, actualResult.Content);
        }

        [Test, TestCaseSource("ResponseHeaders")]
        public void TestDownload_NoLanguage(IList<Parameter> responseHeaders)
        {
            IRestRequest actualRequest = null;
            var languages = "en,pt,es";
            var expectedHash = "ffd8d4aa68033dc03d1c8ef373b9028c";
            var expectedSubtitleContent = "subtitle content";

            var mockResponse = new Mock<IRestResponse>();
            mockResponse.SetupGet(x => x.StatusCode).Returns(HttpStatusCode.OK);
            mockResponse.SetupGet(x => x.Content).Returns(expectedSubtitleContent);
            mockResponse.SetupGet(x => x.Headers).Returns(responseHeaders);

            mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>(r => actualRequest = r)
                .Returns(mockResponse.Object);

            var target = new SubDBClient(mockRestClient.Object);
            var actualResult = target.Download(expectedHash, languages);

            this.AssertRequest(actualRequest, Method.GET, "download", expectedHash);
            Assert.IsNotNull(actualResult);
            Assert.IsNull(actualResult.Language);
            Assert.IsFalse(actualResult.NotFound);
            Assert.AreEqual(expectedSubtitleContent, actualResult.Content);
        }

        [Test]
        public void TestDownload_NotFound()
        {
            IRestRequest actualRequest = null;
            var languages = "en,pt,es";
            var expectedHash = "ffd8d4aa68033dc03d1c8ef373b9028c";

            var mockResponse = new Mock<IRestResponse>();
            mockResponse.SetupGet(x => x.StatusCode).Returns(HttpStatusCode.NotFound);

            mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>(r => actualRequest = r)
                .Returns(mockResponse.Object);

            var target = new SubDBClient(mockRestClient.Object);
            var actualResult = target.Download(expectedHash, languages);

            this.AssertRequest(actualRequest, Method.GET, "download", expectedHash);
            Assert.IsNotNull(actualResult);
            Assert.IsTrue(actualResult.NotFound);
        }

        [Test]
        public void TestDownload_Null()
        {
            IRestRequest actualRequest = null;
            var languages = "en,pt,es";
            var expectedHash = "ffd8d4aa68033dc03d1c8ef373b9028c";

            var mockResponse = new Mock<IRestResponse>();
            mockResponse.SetupGet(x => x.StatusCode).Returns(HttpStatusCode.BadRequest);

            mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>(r => actualRequest = r)
                .Returns(mockResponse.Object);

            var target = new SubDBClient(mockRestClient.Object);
            var actualResult = target.Download(expectedHash, languages);

            this.AssertRequest(actualRequest, Method.GET, "download", expectedHash);
            Assert.IsNull(actualResult);
        }

        [Test]
        public void TestListLanguages()
        {
            IRestRequest actualRequest = null;
            var expectedResult = "en,pt,es";

            var mockResponse = new Mock<IRestResponse>();
            mockResponse.SetupGet(x => x.StatusCode).Returns(HttpStatusCode.OK);
            mockResponse.SetupGet(x => x.Content).Returns(expectedResult);

            mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>(r => actualRequest = r)
                .Returns(mockResponse.Object);

            var target = new SubDBClient(mockRestClient.Object);
            var result = target.ListLanguages();
            var actualResult = string.Join(",", result);

            this.AssertRequest(actualRequest, Method.GET, "languages");
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        public void TestListLanguages_BadRequest()
        {
            IRestRequest actualRequest = null;

            var mockResponse = new Mock<IRestResponse>();
            mockResponse.SetupGet(x => x.StatusCode).Returns(HttpStatusCode.BadRequest);

            mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>(r => actualRequest = r)
                .Returns(mockResponse.Object);

            var target = new SubDBClient(mockRestClient.Object);
            var result = target.ListLanguages();

            this.AssertRequest(actualRequest, Method.GET, "languages");
            Assert.IsNull(result);
        }

        [Test]
        public void TestSearch()
        {
            IRestRequest actualRequest = null;
            var expectedResult = "en,pt,es";
            var expectedHash = "ffd8d4aa68033dc03d1c8ef373b9028c";

            var mockResponse = new Mock<IRestResponse>();
            mockResponse.SetupGet(x => x.StatusCode).Returns(HttpStatusCode.OK);
            mockResponse.SetupGet(x => x.Content).Returns(expectedResult);

            mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>(r => actualRequest = r)
                .Returns(mockResponse.Object);

            var target = new SubDBClient(mockRestClient.Object);
            var result = target.Search(expectedHash, false);
            var actualResult = string.Join(",", result);

            this.AssertRequest(actualRequest, Method.GET, "search", expectedHash);
            Assert.IsFalse(actualRequest.Parameters.Any(x => x.Name == "versions"));
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        public void TestSearch_InvalidHash()
        {
            var target = new SubDBClient(mockRestClient.Object);
            Assert.Throws<ArgumentException>(() => target.Search(string.Empty, false));
        }

        [Test]
        public void TestSearch_NotFound()
        {
            IRestRequest actualRequest = null;
            var expectedResult = new List<string>();
            var expectedHash = "ffd8d4aa68033dc03d1c8ef373b9028c";

            var mockResponse = new Mock<IRestResponse>();
            mockResponse.SetupGet(x => x.StatusCode).Returns(HttpStatusCode.NotFound);

            mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>(r => actualRequest = r)
                .Returns(mockResponse.Object);

            var target = new SubDBClient(mockRestClient.Object);
            var actualResult = target.Search(expectedHash, false);

            this.AssertRequest(actualRequest, Method.GET, "search", expectedHash);
            Assert.IsFalse(actualRequest.Parameters.Any(x => x.Name == "versions"));
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        public void TestSearch_WithFileBytes()
        {
            var basePath = NUnit.Framework.TestContext.CurrentContext.TestDirectory;
            var dexterFile = Path.Combine(basePath, "dexter.mp4");
            var dexterFileBytes = File.ReadAllBytes(dexterFile);

            IRestRequest actualRequest = null;
            var expectedResult = "en,pt,es";
            var expectedHash = "ffd8d4aa68033dc03d1c8ef373b9028c";

            var mockResponse = new Mock<IRestResponse>();
            mockResponse.SetupGet(x => x.StatusCode).Returns(HttpStatusCode.OK);
            mockResponse.SetupGet(x => x.Content).Returns(expectedResult);

            mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>(r => actualRequest = r)
                .Returns(mockResponse.Object);

            var target = new SubDBClient(mockRestClient.Object);
            var result = target.Search(dexterFileBytes, false);

            var actualResult = string.Join(",", result);

            this.AssertRequest(actualRequest, Method.GET, "search", expectedHash);
            Assert.IsFalse(actualRequest.Parameters.Any(x => x.Name == "versions"));
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        public void TestSearch_WithFileBytes_InvalidLength()
        {
            var target = new SubDBClient(mockRestClient.Object);
            var fileBytes = new byte[10];
            Assert.Throws<ArgumentException>(() => target.Search(fileBytes, false));
        }

        [Test]
        public void TestSearch_WithFileBytes_Null()
        {
            var target = new SubDBClient(mockRestClient.Object);
            Assert.Throws<ArgumentNullException>(() => target.Search((byte[])null, false));
        }

        [Test]
        public void TestSearch_WithStream()
        {
            var basePath = NUnit.Framework.TestContext.CurrentContext.TestDirectory;
            var dexterFile = Path.Combine(basePath, "dexter.mp4");

            IRestRequest actualRequest = null;
            var expectedResult = "en,pt,es";
            var expectedHash = "ffd8d4aa68033dc03d1c8ef373b9028c";

            var mockResponse = new Mock<IRestResponse>();
            mockResponse.SetupGet(x => x.StatusCode).Returns(HttpStatusCode.OK);
            mockResponse.SetupGet(x => x.Content).Returns(expectedResult);

            mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>(r => actualRequest = r)
                .Returns(mockResponse.Object);

            var target = new SubDBClient(mockRestClient.Object);
            IEnumerable<string> result = null;
            using (var stream = File.Open(dexterFile, FileMode.Open))
            {
                result = target.Search(stream, false);
            }
            var actualResult = string.Join(",", result);

            this.AssertRequest(actualRequest, Method.GET, "search", expectedHash);
            Assert.IsFalse(actualRequest.Parameters.Any(x => x.Name == "versions"));
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        public void TestSearch_WithStream_CannotRead()
        {
            var target = new SubDBClient(mockRestClient.Object);
            var mockStream = new Mock<Stream>();
            mockStream.SetupGet(x => x.CanRead).Returns(false);
            mockStream.SetupGet(x => x.CanSeek).Returns(true);
            mockStream.SetupGet(x => x.Length).Returns(1);
            Assert.Throws<ArgumentException>(() => target.Search(mockStream.Object, false));
        }

        [Test]
        public void TestSearch_WithStream_CannotSeek()
        {
            var target = new SubDBClient(mockRestClient.Object);
            var mockStream = new Mock<Stream>();
            mockStream.SetupGet(x => x.CanRead).Returns(true);
            mockStream.SetupGet(x => x.CanSeek).Returns(false);
            mockStream.SetupGet(x => x.Length).Returns(1);
            Assert.Throws<ArgumentException>(() => target.Search(mockStream.Object, false));
        }

        [Test]
        public void TestSearch_WithStream_InvalidLength()
        {
            var target = new SubDBClient(mockRestClient.Object);
            var mockStream = new Mock<Stream>();
            mockStream.SetupGet(x => x.CanRead).Returns(true);
            mockStream.SetupGet(x => x.CanSeek).Returns(true);
            mockStream.SetupGet(x => x.Length).Returns(1);
            Assert.Throws<ArgumentException>(() => target.Search(mockStream.Object, false));
        }

        [Test]
        public void TestSearch_WithStream_Null()
        {
            var target = new SubDBClient(mockRestClient.Object);
            Assert.Throws<ArgumentNullException>(() => target.Search((Stream)null, false));
        }

        [Test]
        public void TestSearch_WithVersions()
        {
            IRestRequest actualRequest = null;
            var expectedResult = "en:1,pt:2,es:3";
            var expectedHash = "ffd8d4aa68033dc03d1c8ef373b9028c";

            var mockResponse = new Mock<IRestResponse>();
            mockResponse.SetupGet(x => x.StatusCode).Returns(HttpStatusCode.OK);
            mockResponse.SetupGet(x => x.Content).Returns(expectedResult);

            mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>(r => actualRequest = r)
                .Returns(mockResponse.Object);

            var target = new SubDBClient(mockRestClient.Object);
            var result = target.Search(expectedHash, true);
            var actualResult = string.Join(",", result);

            this.AssertRequest(actualRequest, Method.GET, "search", expectedHash);
            Assert.IsTrue(actualRequest.Parameters.Any(x => x.Name == "versions"));
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        public void TestUpload_Duplicated()
        {
            IRestRequest actualRequest = null;
            var hash = "ffd8d4aa68033dc03d1c8ef373b9028c";
            var expectedResult = SubDBUploadResponse.Duplicated;

            var mockResponse = new Mock<IRestResponse>();
            mockResponse.SetupGet(x => x.StatusCode).Returns(HttpStatusCode.Forbidden);

            mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>(r => actualRequest = r)
                .Returns(mockResponse.Object);

            var target = new SubDBClient(mockRestClient.Object);
            var result = target.Upload(hash, mockSubtitleBytes);

            this.AssertRequest(actualRequest, Method.POST, "upload", hash);
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TestUpload_Error()
        {
            IRestRequest actualRequest = null;
            var hash = "ffd8d4aa68033dc03d1c8ef373b9028c";
            var expectedResult = SubDBUploadResponse.Error;

            var mockResponse = new Mock<IRestResponse>();
            mockResponse.SetupGet(x => x.StatusCode).Returns(HttpStatusCode.BadRequest);

            mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>(r => actualRequest = r)
                .Returns(mockResponse.Object);

            var target = new SubDBClient(mockRestClient.Object);
            var result = target.Upload(hash, mockSubtitleBytes);

            this.AssertRequest(actualRequest, Method.POST, "upload", hash);
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TestUpload_Invalid()
        {
            IRestRequest actualRequest = null;
            var hash = "ffd8d4aa68033dc03d1c8ef373b9028c";
            var expectedResult = SubDBUploadResponse.Invalid;

            var mockResponse = new Mock<IRestResponse>();
            mockResponse.SetupGet(x => x.StatusCode).Returns(HttpStatusCode.UnsupportedMediaType);

            mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>(r => actualRequest = r)
                .Returns(mockResponse.Object);

            var target = new SubDBClient(mockRestClient.Object);
            var result = target.Upload(hash, mockSubtitleBytes);

            this.AssertRequest(actualRequest, Method.POST, "upload", hash);
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TestUpload_WithBothFileBytes()
        {
            var basePath = NUnit.Framework.TestContext.CurrentContext.TestDirectory;
            var dexterFile = Path.Combine(basePath, "dexter.mp4");
            var dexterFileBytes = File.ReadAllBytes(dexterFile);

            IRestRequest actualRequest = null;
            var expectedHash = "ffd8d4aa68033dc03d1c8ef373b9028c";
            var expectedResult = SubDBUploadResponse.Uploaded;

            var mockResponse = new Mock<IRestResponse>();
            mockResponse.SetupGet(x => x.StatusCode).Returns(HttpStatusCode.Created);

            mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>(r => actualRequest = r)
                .Returns(mockResponse.Object);

            var target = new SubDBClient(mockRestClient.Object);
            var result = target.Upload(dexterFileBytes, mockSubtitleBytes);

            this.AssertRequest(actualRequest, Method.POST, "upload", expectedHash);
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TestUpload_WithBothStreams()
        {
            var basePath = NUnit.Framework.TestContext.CurrentContext.TestDirectory;
            var dexterFile = Path.Combine(basePath, "dexter.mp4");

            IRestRequest actualRequest = null;
            var expectedHash = "ffd8d4aa68033dc03d1c8ef373b9028c";
            var expectedResult = SubDBUploadResponse.Uploaded;

            var mockResponse = new Mock<IRestResponse>();
            mockResponse.SetupGet(x => x.StatusCode).Returns(HttpStatusCode.Created);

            mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>(r => actualRequest = r)
                .Returns(mockResponse.Object);

            var target = new SubDBClient(mockRestClient.Object);
            SubDBUploadResponse result = SubDBUploadResponse.None;

            using (FileStream fs = new FileStream(dexterFile, FileMode.Open))
            {
                using (MemoryStream subtitleStream = new MemoryStream(this.mockSubtitleBytes))
                {
                    result = target.Upload(fs, subtitleStream);
                }
            }

            this.AssertRequest(actualRequest, Method.POST, "upload", expectedHash);
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TestUpload_WithHashAndFileBytes()
        {
            IRestRequest actualRequest = null;
            var hash = "ffd8d4aa68033dc03d1c8ef373b9028c";
            var expectedResult = SubDBUploadResponse.Uploaded;

            var mockResponse = new Mock<IRestResponse>();
            mockResponse.SetupGet(x => x.StatusCode).Returns(HttpStatusCode.Created);

            mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>(r => actualRequest = r)
                .Returns(mockResponse.Object);

            var target = new SubDBClient(mockRestClient.Object);
            var result = target.Upload(hash, mockSubtitleBytes);

            this.AssertRequest(actualRequest, Method.POST, "upload", hash);
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TestUpload_WithHashAndFileStream()
        {
            IRestRequest actualRequest = null;
            var hash = "ffd8d4aa68033dc03d1c8ef373b9028c";
            var expectedResult = SubDBUploadResponse.Uploaded;

            var mockResponse = new Mock<IRestResponse>();
            mockResponse.SetupGet(x => x.StatusCode).Returns(HttpStatusCode.Created);

            mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>(r => actualRequest = r)
                .Returns(mockResponse.Object);

            var target = new SubDBClient(mockRestClient.Object);
            SubDBUploadResponse result = SubDBUploadResponse.None;

            using (MemoryStream ms = new MemoryStream(mockSubtitleBytes))
            {
                result = target.Upload(hash, ms);
            }

            this.AssertRequest(actualRequest, Method.POST, "upload", hash);
            Assert.AreEqual(expectedResult, result);
        }

        #endregion Public Methods

        #region Private Methods

        private void AssertRequest(IRestRequest actual, Method expectedMethod, string expectedAction, string expectedHash = null)
        {
            Assert.AreEqual(expectedMethod, actual.Method);

            var actionParameter = actual.Parameters.SingleOrDefault(x => x.Name == "action");
            var hashParameter = actual.Parameters.SingleOrDefault(x => x.Name == "hash");

            Assert.IsNotNull(actionParameter);
            Assert.AreEqual(expectedAction, actionParameter.Value);

            if (!string.IsNullOrWhiteSpace(expectedHash))
            {
                Assert.IsNotNull(hashParameter);
                Assert.AreEqual(expectedHash, hashParameter.Value);
            }
        }

        #endregion Private Methods
    }
}