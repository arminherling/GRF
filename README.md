# GRF

Library for loading Ragnarok Online GRF Files.

[![CodeFactor](https://www.codefactor.io/repository/github/arminherling/grf/badge?style=plastic)](https://www.codefactor.io/repository/github/arminherling/grf) [![Build status](https://ci.appveyor.com/api/projects/status/325bija509s7wnp0?svg=true)](https://ci.appveyor.com/project/arminherling/grf) [![Build Status](https://travis-ci.org/arminherling/GRF.svg?branch=master)](https://travis-ci.org/arminherling/GRF) [![license](https://img.shields.io/github/license/arminherling/GRF.svg)](https://github.com/arminherling/GRF/blob/master/LICENSE) [![NuGet](https://img.shields.io/nuget/v/GRF.svg)](https://www.nuget.org/packages/GRF/)


 * [Changelog](CHANGELOG.md)

## Supported GRF File Versions

- [x] Version 0x102
- [x] Version 0x103
- [x] Version 0x200

## Supported Features

- [x] Lazy loading of GRF Files
- [x] Eager loading of GRF Files

## Usage Example

### Loading single GRF Files

```cs
  // Use the static FromFile method to load the GRF file
  var grf = Grf.FromFile( @"RO\test.grf" );

  // Get the GRF entry from the file
  var entryWasFound = grf.Find( "data\\idnum2itemdisplaynametable.txt", out GrfEntry entry );

  // Write the data from the entry to a file
  File.WriteAllBytes( @"directory\file.txt", entry.GetUncompressedData() );

```

### Loading multiple GRF Files

```cs
  // Use the static FromFile method and pass ini file path into the method to load the collection
  var collection = GrfCollection.FromFile( @"RO\data.ini" );

  // Find the correct entry from all loaded GRF files.
  // This method behaves just like the game, entries from grf files with a higher 
  // priority hide entries with the same name in grf files with lower priorities
  var entryWasFound = collection.Find( "data\\idnum2itemdisplaynametable.txt", out GrfEntry entry );

  // Write the data from the entry to a file
  File.WriteAllBytes( @"directory\file.txt", entry.GetUncompressedData() );

```
