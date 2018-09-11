# GRF

Library for loading Ragnarok Online GRF Files.

[![license](https://img.shields.io/github/license/arminherling/GRF.svg)](https://github.com/arminherling/GRF/blob/master/LICENSE) [![Build Status](https://travis-ci.org/arminherling/GRF.svg?branch=master)](https://travis-ci.org/arminherling/GRF) 

 * [Changelog](CHANGELOG.md)

## Supported GRF File Versions

- [ ] Version 0x102
- [ ] Version 0x103
- [x] Version 0x200

## Supported Features

- [x] Loading GRF Files
- [ ] Saving GRF Files
- [ ] Adding new entries to GRF Files

## Usage Example

### Loading GRF Files

```cs
  // create a new GRF object and use the Load method
  var grf = new Grf();
  grf.Load( @"Data\test.grf" );

  // alternatively you can pass the path into the constructor
  var grf = new Grf( @"Data\test.grf" );

  // get the GRF entry from the file
  var entry = grf.Files["data\\idnum2itemdisplaynametable.txt"];

  // write the data from the entry to a file
  File.WriteAllBytes( "directory\\file.txt", entry.GetUncompressedData() );

```
