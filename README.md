# bitopro-api-csharp <img src="https://www.bitopro.com/ns/images/logo-light.svg" width="150" />

## C# Wrapper for the official Bitopro exchange API

Compatible with **.NET 4.7.2,.NETSTANDARD2.0**

This repository provides a C# wrapper for the official Bitopro API, all `REST` and `WEBSOCKET` endpoints covered, and a best practice solution coupled with strongly typed responses and requests. It is built on .NET Framework

Feel free to raise issues and Pull Request to help improve the library. If you found this API useful, and you wanted to give back to Bitopro via our Telegram group [**here**](https://t.me/BitoProOfficial).

## Documentation
- [Bitopro API Doc](https://github.com/bitoex/bitopro-offical-api-docs)
- [REST API](https://github.com/bitoex/bitopro-offical-api-docs#restful-api-list)
- [WebSocket API](https://github.com/bitoex/bitopro-offical-api-docs#websocket-stream-list)



## Contribution
```git
git https://github.com/bitoex/bitopro-api-csharp.git
```
- Navigate to `Program.cs`
- Add your own Private and Secret keys
- Play around with the API

## Features
- Simple, Configurable, Extendable
- Rate limiting, 600 requests per minute per IP
- `log4net` Interfaces supported
- dotnet standard, 4.7.2 support
- Bitopro WebSockets
- Console app with examples ready to launch _(provide API keys)_

## Examples
All examples are available to play around with within the repositorys Console application which can be found in `Program.cs`.

## Usage
Code examples below, or clone the repository and run the  project.

### Creating a Client
General usage just requires setting up the client with your credentials, and then calling the Client as necessary.
```c#
// Build out a client, provide a logger, and more configuration options, or even your own APIProcessor implementation
var bitopro = new BitoproRestApi(API_KEY, API_SECRET);

//You an also specify symbols like this.
var string SYMBOL = "BTC_USDT";
dynamic response;
response = await bitopro.GetCurrencies();
```

### Creating a WebSocket Client
For WebSocket endpoints, just instantiate the BitoproWebsocket.
You can use a `using` block or manual management.
```c#
// [PUBLIC] GET Order book
var bitoproWsOrderBook = new BitoproOrderBookWebsocket(symbolsLimit);
bitoproWsOrderBook.On_Receive_Message += Receive_WsMessage;
bitoproWsOrderBook.InitWebsocket();

static void Receive_WsMessage(string message)
{
    Utils.Logger.Info(message);
}
```
