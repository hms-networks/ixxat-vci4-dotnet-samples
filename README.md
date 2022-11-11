# VCI4 .NET Samples

## Introduction

This repository contains a set sample programs which demonstrates the 
functionality of the Ixxat.Vci4 Nuget package,
for details see https://www.nuget.org/packages/Ixxat.Vci4

VCI4 (VCI4 = Virtual Communication Interface Version 4) is a driver/application framework to 
access HMS CAN or LIN interfaces on the Windows OS,
for details see https://www.ixxat.com/technical-support/support/windows-driver-software

## Prerequisites

  - Visual Studio 2022
  - a valid VCI4 installation

## Usage

Open the solution in src/Samples.sln and compile/run projects.

### Source tree overview

    \src\net40     projects targeting .NET framework (net48)
    \src\net50     projects targeting .NET core (net5.0-windows)
    \src\net60     projects targeting .NET core (net6.0-windows)

### Examples

#### CanConNet

Simple console examples demonstrates the use of the older (CAN only) APIs.
This example is provided as reference and it is advised to use the new CAN FD APIs
as they support pure CAN interfaces, too.

#### CanFdConNet 

Simple console examples like CanConNet, but uses the current APIs which 
also supports CAN FD.
The new APIs differ in how (CAN FD) bitrates are specified and 
supports a CAN FD data frame type. As CAN is a subset of CAN FD the 
new APIs support pure CAN interfaces, too, and you can specify CAN bitrates without 
BRS (bitrate switch) or read/write normal CAN frames.

#### LinConNet 

Simple console examples to demonstrate the APIs to access Lin interfaces.

#### DeviceEnumerator

GUI sample which demontrates the use of the device enumerator.
Shows a list of devices and updates the list automatically when a removable
CAN device is plugged in/plugged out.

## References

[CAN]     https://en.wikipedia.org/wiki/CAN_bus  
[CAN FD]  https://en.wikipedia.org/wiki/CAN_FD  
[Lin]     https://en.wikipedia.org/wiki/Local_Interconnect_Network  