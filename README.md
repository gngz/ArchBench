
# ArchBench
O ArchBench é uma bancada para testar alguns padrões e táticas arquiteturais. Neste projeto, o ArchBench foi utilizado de forma a desenvolver um Broker. Foram desenvolvidos alguns plugins extras tais como: Serve e RequestTester.

## Broker
É o plugin que tem objetivo de encaminhar os pedidos para um dos servidores.
Possui nas definições a propriedade Algorithm, que permite escolher um dos algoritmos de encaminhamento. É possível escolher um destes valores:
- roundrobin - Encaminha para o servidor seguinte na lista.
- sameserver - Encaminha sempre para o mesmo servidor.

### Features
- Suporta 2 tipos de algoritmos de encaminhamento (mas extensível).
- Suporta Form Encoding: URL Encoded, Multipart Forms.
- Upload de Ficheiros.

## Broker Register
É o plugin que permite o registo do servidor no broker. Possui duas definições:
 - BrokerAddress - Endereço do broker.
 - ServerPort - A porta que o servidor web está a utilizar.

## Serve
Serve é um plugin que funciona como um pequeno servidor web de páginas estáticas a partir de um directório à escolha. A escolha do diretório é feita nas definições do Plugin.

### Features
- Indexação dos diretórios que não possuam um index.html.

## RequestTester
O RequestTester é um plugin que mostra alguns parâmetros de um pedido HTTP, tais como: Headers, Cookies, Formulários, Ficheiros, etc..
