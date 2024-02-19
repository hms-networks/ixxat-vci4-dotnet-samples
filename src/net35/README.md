# VCI4 .NET Samples (.Net 3.5/, VS2008)

## Introduction

This subdirectory contains samples for the VS2008/.NET 3.5 target which references the
the Ixxat.Vci4.net35 Nuget package,
for details see https://www.nuget.org/packages/Ixxat.Vci4.nuget35

VCI4 (VCI4 = Virtual Communication Interface Version 4) is a driver/application framework to 
access HMS CAN or LIN interfaces on the Windows OS,
for details see https://www.ixxat.com/technical-support/support/windows-driver-software

## Prerequisites

  - Visual Studio 2008
  - a valid VCI4 installation

## Usage

Execute the getpackages.ps1 script, which downloads the NuGet client and then
downloads/unpacks the Ixxat.Vci4.net35 Nuget package.
After that it copies the native assemblies to the project output directories to 
keep the resulting samples runable.

Open the solution in SamplesNet35.sln and compile/run projects.

### Examples

#### CanConNet

Simple console examples demonstrates the use of the older (CAN only) APIs.
This example is provided as reference and it is advised to use the new CAN FD APIs
as they support pure CAN interfaces, too.

#### LinConNet 

Simple console examples to demonstrate the APIs to access Lin interfaces.

## References

[CAN]     https://en.wikipedia.org/wiki/CAN_bus  
[CAN FD]  https://en.wikipedia.org/wiki/CAN_FD  
[Lin]     https://en.wikipedia.org/wiki/Local_Interconnect_Network  