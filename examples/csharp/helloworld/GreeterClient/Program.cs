// Copyright 2015 gRPC authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Helloworld;

namespace GreeterClient
{
    class Program
    {
        public static void Main(string[] args)
        {
            Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);

            var client = new Greeter.GreeterClient(channel);
            String user = "you";

            var reply = client.SayHello(new HelloRequest { Name = user });
            Console.WriteLine("Greeting: " + reply.Message);

            //StreamReply(client, user).Wait();

            Stream(client, user).Wait();

            channel.ShutdownAsync().Wait();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        public static async Task StreamReply(Greeter.GreeterClient client, String user)
        {
          var sreply = client.SayHelloStreamReply(new HelloRequest { Name = "Stream " + user });

          while (await sreply.ResponseStream.MoveNext(CancellationToken.None))
          {
            var rep = sreply.ResponseStream.Current;
            Console.WriteLine("Greeting Async: " + rep.Message);
          }
        }

    public static async Task Stream(Greeter.GreeterClient client, String user)
    {
      var sreply = client.SayHelloStream();
      
      await sreply.RequestStream.WriteAsync(new HelloRequest { Name = "Stream " + user });

      int i = 0;
      while (await sreply.ResponseStream.MoveNext(CancellationToken.None))
      {
        var rep = sreply.ResponseStream.Current;
        Console.WriteLine("Greeting Async: " + rep.Message);
        if (i++ < 10)
        {
          await sreply.RequestStream.WriteAsync(new HelloRequest { Name = i + " Stream " + user });
        }
      }
    }
  }
}
