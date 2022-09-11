# REWE-eBon-Parser
This is a small tool that takes REWE (https://rewe.de) electronic receipt PDF files and parses them for your enjoyment. 

I am using this as part of a toolchain that takes new receipts, parses them and published all information to a database for later use.

## Prerequisites
You need to fulfill some requirements before you can use this tool:

### Software
This is a .net 6.0 program that requires one NUget dependency (PdfPig / https://github.com/UglyToad/PdfPig). 
You can either run and compile it yourself - either directly in Visual Studio or using Docker.

### REWE
You need to enable the electronic PDF receipts in your REWE account by connecting a bonus card (PAYBACK) to the account.
Then you can activate the "eBon" in the settings and you'll receive the receipt after every purchase via Mail.

## Using

Either you use the code itself - like the actual library - and the Parser Program as the example code.

Or you use the Parser Program itself. It'll expect some parameters when you start it:

1: the directory to search for pdf eBon files
2: the IP adress of your mqtt broker (to publish the receipts contents to)
3: the mqtt topic prefix

Usage: REWEeBonParser.exe <directory to search for PDF eBon files> <mqtt-broker-ip> <mqtt-topic-prefix with / at the end>

When you start the program this way it'll go through all pdf files in the directory and if it finds REWE eBons it will read them in. It will then order the eBons by date and output all of them in the correct timely order to MQTT. 

Then it will start watching the directory for any changes and new files. It'll pick up those files automatically, read them in and send the data to MQTT of the receipt date is newer than the last one seen and sent.

It will create a lastknown.cfg file in the directory of the PDF files (it should have write rights there therefore!) and store the "last seen receipt date" in there.

### Docker

Step 1: build the docker image with the included Dockerfile

Step 2: run

docker run -d -it --volume $pathtoreceipts:/receipts --name rewe-ebon-parser rewe-ebon-parser /receipts mqttbroker "external/rewe/"

