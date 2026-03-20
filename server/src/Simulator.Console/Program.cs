using Simulator.Core.Geometry;
using Simulator.Core.Geometry.Utils;
using Simulator.IO;

namespace Simulator.Console;

public class Program
{
    static void Main(string[] args)
    {
        System.Console.WriteLine("Welcome to the evacuation simulator CLI interface! Select an option by typing the desired number.");
        var complete = false;
        while (!complete)
        {
            System.Console.WriteLine("\t1. View and store triangulation");
            System.Console.WriteLine("\t2. Exit");
            var option = System.Console.ReadLine();

            switch (option)
            {
                case "1":
                    System.Console.Write("Path to geometry input: ");
                    var inPath = System.Console.ReadLine();
                    if (string.IsNullOrEmpty(inPath))
                    {
                        System.Console.WriteLine("Must provide a valid input path");
                        break;
                    }

                    System.Console.Write("Path to triangulation output: ");
                    var outPath = System.Console.ReadLine();
                    if (string.IsNullOrEmpty(outPath))
                    {
                        System.Console.WriteLine("Must provide a valid output path");
                        break;
                    }
                    
                    System.Console.Write("Geometry exclusion radius (default 22.5): ");
                    var exclusionRadStr = System.Console.ReadLine();
                    double exclusionRad;
                    if (string.IsNullOrEmpty(exclusionRadStr))
                        exclusionRad = 22.5;
                    else
                        exclusionRad = double.Parse(exclusionRadStr);
                    
                    var inputGeometry = inPath.Deserialise<InputGeometry>();
                    var mesh = NavMeshGenerator.GenerateNavMesh(inputGeometry, 50, exclusionRad);
                    mesh.Save(outPath, 1);
                    System.Console.WriteLine("Triangulation saved to disk");
                    break;
                case "2":
                    complete = true;
                    break;
            }
        }
    }
}