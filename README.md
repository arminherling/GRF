# GRF

Library for loading Ragnarok Online GRF Files.

[![CodeFactor](https://www.codefactor.io/repository/github/arminherling/grf/badge?style=plastic)](https://www.codefactor.io/repository/github/arminherling/grf) [![Build status](https://ci.appveyor.com/api/projects/status/325bija509s7wnp0?svg=true)](https://ci.appveyor.com/project/arminherling/grf) [![Build Status](https://travis-ci.org/arminherling/GRF.svg?branch=master)](https://travis-ci.org/arminherling/GRF) [![license](https://img.shields.io/github/license/arminherling/GRF.svg)](https://github.com/arminherling/GRF/blob/master/LICENSE) [![NuGet](https://img.shields.io/nuget/v/GRF.svg)](https://www.nuget.org/packages/GRF/)


 * [Changelog](CHANGELOG.md)

## Supported GRF File Versions

- [x] Version 0x102
- [x] Version 0x103
- [x] Version 0x200

## Supported Features

- [x] Loading GRF Files
- [ ] Saving GRF Files
- [ ] Adding new entries to GRF Files

## Usage Example

### Loading single GRF Files

```cs
  // Create a new GRF object and pass grf path file the Load method
  var grf = new Grf();
  grf.Load( @"RO\test.grf" );

  // Alternatively you can pass the path into the constructor
  var grf = new Grf( @"RO\test.grf" );

  // Get the GRF entry from the file
  var entry = grf.Entries[ "data\\idnum2itemdisplaynametable.txt" ];

  // Write the data from the entry to a file
  File.WriteAllBytes( @"directory\file.txt", entry.GetUncompressedData() );

```

### Loading multiple GRF Files

```cs
  // Create a new GRF collection and pass ini file path to the Load method
  var collection = new GrfCollection();
  collection.Load( @"RO\data.ini" );

  // Alternatively you can pass the path into the constructor
  var collection = new GrfCollection( @"RO\data.ini" );

  // Find the correct entry from all loaded GRF files.
  // This method behaves just like the game, entries from grf files with a higher 
  // priority hide entries with the same name in grf files with lower priorities
  var entryWasFound = collection.FindEntry( "data\\idnum2itemdisplaynametable.txt", out GrfEntry entry );

  // Write the data from the entry to a file
  File.WriteAllBytes( @"directory\file.txt", entry.GetUncompressedData() );

```
