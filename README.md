# GRF

Library for loading Ragnarok Online GRF Files.

[![Build Status](https://travis-ci.org/arminherling/GRF.svg?branch=master)](https://travis-ci.org/arminherling/GRF) [![license](https://img.shields.io/github/license/arminherling/GRF.svg)](https://github.com/arminherling/GRF/blob/master/LICENSE) [![NuGet](https://img.shields.io/nuget/v/GRF.svg)](https://www.nuget.org/packages/GRF/)


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
  // create a new GRF object and pass grf path file the Load method
  var grf = new Grf();
  grf.Load( @"RO\test.grf" );

  // alternatively you can pass the path into the constructor
  var grf = new Grf( @"RO\test.grf" );

  // get the GRF entry from the file
  var entry = grf.Entries[ "data\\idnum2itemdisplaynametable.txt" ];

  // write the data from the entry to a file
  File.WriteAllBytes( @"directory\file.txt", entry.GetUncompressedData() );

```

### Loading multiple GRF Files

```cs
  // create a new GRF collection and pass ini file path to the Load method
  var collection = new GrfCollection();
  collection.Load( @"RO\data.ini" );

  // alternatively you can pass the path into the constructor
  var collection = new GrfCollection( @"RO\data.ini" );

  // Find the correct entry from all loaded GRF files.
  // This method behaves just like the game, entries from grf files with a higher 
  // priority hide entries with the same name in grf files with lower priorities
  var entryWasFound = collection.FindEntry( "data\\idnum2itemdisplaynametable.txt", out GrfEntry entry );

  // write the data from the entry to a file
  File.WriteAllBytes( @"directory\file.txt", entry.GetUncompressedData() );

```
