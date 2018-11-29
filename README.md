# C# XMLDocument To Html
This program provides a method for converting an XML document that can be output from C# Compiler to a user defined HTML document.  

# Development history and goals
C# XML Document outputs Doc comment written in source code by XML, but it is not suitable for people to read.  
It is ideal that it can be seen using HTML output such as Java and using any Web browser (application installed as a standard application), unfortunately it is not included in C# XML Document. HTML may be implemented by handwriting, but it is inconvenient to do it every time, ideal is to generate HTML with less operation.  
I think inconvenienced when I create my own document.  
So it is the goal of this program and the project to analyze the XML outputted by the C# XML Document, convert it to HTML format, and arrange it so that everyone can read it.  

## Development environment
**Windows**
1. Visual Studio 2017
2. .Net Framework 4.7.1

**Mac**
1. Visual Studio for Mac

## Minimum environment required for operation
1. .Net Framework 4.7.1

## Usage
**CUI**
```
XMLDocumentToHtmlCUI.exe -b {TemplateBaseDir} -o {OutputDir} {XmlDocuments}
example: XMLDocumentToHtmlCUI.exe -o Out XmlDocument1.xml XmlDocument2.xml
```

**Options**  

| Option | Description | Default |
|-----|:----|:----|
|-b   |Specify the directory where the template file is stored. If you specify this, you can output with your own template.|BaseTemplate|
|-o   |Change the directory path of the output destination.|{Directory of executable file}/Root|

**Argument**  
Specifies one or more XML Docment outputted by C# XML Document.  

## Build
1. Open XMLDocumentToHtmlCUI.sln
2. Click the Build button on the menu
