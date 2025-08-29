using System;
using LDtkMonogame;

// Test program to explore LDtkMonoGame API
namespace TestLdtk
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Testing LDtkMonoGame API...");
            
            // Try to create an LDtk world or project
            // This will help us understand the API structure
            try
            {
                // Explore available types
                Console.WriteLine("Available LDtkMonoGame types:");
                var assembly = typeof(LDtkWorld).Assembly;
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsPublic)
                    {
                        Console.WriteLine($"  {type.Name}: {type.FullName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exploring LDtkMonoGame types: {ex.Message}");
            }
        }
    }
}