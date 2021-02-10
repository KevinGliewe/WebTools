#r "nuget:Nuke.Common, 5.0.2"
#load "nuget:Dotnet.Build, 0.7.1"

// https://medium.com/@lakindu1995/installing-net-core-sdk-in-ssh-linux-environment-fec0edf9e116

using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

using Nuke.Common.IO;

using static FileUtils;

Console.WriteLine("Args: " +  String.Join(", ", Args.Select( arg => "'" + arg + "'")));

var scriptFilder = (AbsolutePath)GetScriptFolder();
var projectPath = (AbsolutePath)(Args.Count > 0 ? Path.GetFullPath(Path.GetDirectoryName(Args[0].Trim())) : scriptFilder);
var outPath = (AbsolutePath)(Args.Count > 1 ? Path.GetFullPath(Args[1].Trim()) : projectPath);

Console.WriteLine($"ScriptFilder : {scriptFilder}");
Console.WriteLine($"ProjectPath  : {projectPath}");
Console.WriteLine($"OutPath      : {outPath}");

var toInject = new [] {
    new {
        File = outPath / "wwwroot" / "_framework" / "blazor.webassembly.js",
        Pattern = @"return r\.loadResource\(o,t\(o\),e\[o\],n\)",
        Replace = @"var p = r.loadResource(o,t(o),e[o],n); p.response.then((x) => { if (typeof window.loadResourceCallback === ""function"") { window.loadResourceCallback(Object.keys(e).length, o, x);}}); return p;"
    },
};

var compressionExtensions = new [] { ".br", ".gz" };

void DeleteCompressed(string file) {
    foreach(var ex in compressionExtensions) {
        var cfile = file + ex;
        if(File.Exists(cfile)) {
            File.Delete(cfile);
            Console.WriteLine("  Found and deleted compressed File " + cfile);
        }
    }
}

foreach(var inj in toInject) {
    Console.WriteLine($"Injecting into {inj.File}");

    try {
        var data = File.ReadAllText(inj.File);
        var data_r = Regex.Replace(data, inj.Pattern, inj.Replace);
        File.WriteAllText(inj.File, data_r);

        if( data == data_r )
            Console.WriteLine("!! Could not replace !!");
        else
            DeleteCompressed(inj.File);
    } catch(Exception ex) {
        Console.WriteLine(ex);
    }
}
