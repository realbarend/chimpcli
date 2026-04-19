using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Shouldly;

namespace Chimp.Tests.Api;

public class HttpMessageHandlerMocker
{
    public Mock<HttpMessageHandler> Mock { get; }
    private Queue<HttpRequestMock> RequestMocks { get; } = [];
    private int RequestCount { get; set; }

    public HttpMessageHandlerMocker()
    {
        Mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        Mock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                RequestCount++;
                if (RequestMocks.Count == 0) return new HttpResponseMessage(HttpStatusCode.OK);
                var requestMock = RequestMocks.Dequeue();

                foreach (var assertion in requestMock.Assertions) assertion(request);

                return new HttpResponseMessage(requestMock.StatusCode)
                    { Content = requestMock.ResponseJson == null ? null : new StringContent(JsonSerializer.Serialize(requestMock.ResponseJson, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })) };
            });
    }

    public HttpMessageHandlerMocker AddRequest(HttpRequestMock request)
    {
        RequestMocks.Enqueue(request);
        return this;
    }
}

public class HttpRequestMock
{
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
    public List<Action<HttpRequestMessage>> Assertions { get; private init; } = [];
    public object? ResponseJson { get; private init; }

    public static HttpRequestMock ForRequestPath(string localPath, object? responseBodyJson)
    {
        return new HttpRequestMock
        {
            Assertions = [request => request.RequestUri?.LocalPath.ShouldBe(localPath)],
            ResponseJson = responseBodyJson,
        };
    }
}
