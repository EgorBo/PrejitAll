# PrejitAll

A helper tool to prejit everything in the current folder in just one command. 

# Usage

```ps1
cd folder/with/managed/libs
dotnet prejitall
# or if you want to use a custom crossgen2.exe:
dotnet prejitall C:\prj\runtime-main\artifacts\bin\coreclr\windows.x64.Checked\crossgen2\crossgen2.exe
```
NOTE: target directory is expected to have all possible dependencies (e.g. result of `dotnet publish` command).

# Installation

```
dotnet tool install -g prejitall
```