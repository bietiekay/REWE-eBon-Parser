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
