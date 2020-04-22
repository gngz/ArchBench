
/*

private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
await semaphore.WaitAsync();
semaphore.Release();

                HttpResponseMessage response = null;
                response = await HttpClient.PostAsync(uri, GetFormContent(aRequest));
                response = await HttpClient.GetAsync(uri);

                                if ( response.Content is StreamContent stream )
                {
                    var bytes = await stream.ReadAsByteArrayAsync();
                    aResponse.Body.Write( bytes, 0, bytes.Length );
                }

                        private HttpContent GetFormContent(IHttpRequest aRequest)
        {
            return new FormUrlEncodedContent(
                aRequest.Form.Select(
                    item => new KeyValuePair<string, string>(item.Name, item.Value))
            );
        }

        private void BackwardCookie( IHttpResponse aResponse, HttpResponseMessage aMessage )
        {
            foreach ( var header in aMessage.Headers )
            {
                Console.WriteLine( $"{header.Key} := { string.Join( ", ", header.Value ) }" );
//                aResponse.AddHeader(header.Key, string.Join( ",", header.Value ) );
            }
        }


*/

