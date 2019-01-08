# Changelog

## [Unreleased]

## [0.3.1] - 2019-01-08
### Changed
- Updated System.Memory dependency to 4.5.2

## [0.3.0] - 2019-01-07
### Changed
- Improved memory usage and speed while uncompressing files
- Renamed GrfFile to GrfEntry
- Renamed GrfFile.FilePath to GrfEntry.Path
- Renamed GrfFile.FileName to GrfEntry.Name
- Renamed GrfFile.FileType to GrfEntry.Type
- Renamed Grf.Files to Grf.Entries
- Renamed Grf.FileCount to Grf.EntryCount
- Renamed Grf.FileNames to Grf.EntryNames

### Added
- Loading multiple GRF files with a data.ini file
- Loading entries from GrfCollection depending on the order of loaded GRF files

## [0.2.0] - 2018-09-17
### Changed
- GrfFile.Name to GrfFile.FilePath

### Added
- GrfFile.FileName
- GrfFile.FileType
- Loading Version 0x102 GRF Files
- Loading Version 0x103 GRF Files

## 0.1.1 - 2018-09-11
### Added
- Changelog
- Readme with example usage
- Constructor that accepts file path

## [0.1.0] - 2018-09-10
### Added
- Loading Version 0x200 GRF Files
- NuGet package https://www.nuget.org/packages/GRF/

[Unreleased]: https://github.com/arminherling/GRF/compare/v0.3.1...HEAD
[0.3.1]: https://github.com/arminherling/GRF/compare/v0.3.0...v0.3.1   
[0.3.0]: https://github.com/arminherling/GRF/compare/v0.2.0...v0.3.0   
[0.2.0]: https://github.com/arminherling/GRF/compare/v0.1.0...v0.2.0   
[0.1.0]: https://github.com/arminherling/GRF/compare/a3830e726675b4e4d8fb0c78fef82b898fbc7cb8...v0.1.0