using System;
using NUnit.Framework;
using RestSharp;
using Moq;
using System.Net;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace TheSubDBSharp.Client.Tests
{
    [TestFixture]
    public class TheSubDBSharpTests
    {
        private Mock<IRestClient> mockRestClient = null;

        [OneTimeSetUp]
        public void Setup()
        {
            mockRestClient = new Mock<IRestClient>();
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
        public void TestSearch_InvalidHash()
        {
            var target = new SubDBClient(mockRestClient.Object);
            Assert.Throws<ArgumentException>(() => target.Search(string.Empty, false)); 
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
        public void TestSearch_WithStream_Null()
        {
            var target = new SubDBClient(mockRestClient.Object);
            Assert.Throws<ArgumentNullException>(() => target.Search((Stream)null, false));
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
        public void TestDownload()
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

        private void AssertRequest(IRestRequest actual, Method expectedMethod, string expectedAction, string expectedHash = null)
        {
            Assert.AreEqual(expectedMethod, actual.Method);

            var actionParameter = actual.Parameters.SingleOrDefault(x => x.Name == "action");
            var hashParameter = actual.Parameters.SingleOrDefault(x => x.Name == "hash");
            
            Assert.IsNotNull(actionParameter);
            Assert.AreEqual(expectedAction, actionParameter.Value);
            
            if(!string.IsNullOrWhiteSpace(expectedHash))
            {
                Assert.IsNotNull(hashParameter);
                Assert.AreEqual(expectedHash, hashParameter.Value);
            }
        }
    }
}
