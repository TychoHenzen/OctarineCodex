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
                var ldtkWorld = new LDtkWorld();
                Console.WriteLine("LDtkWorld created successfully");
                Console.WriteLine($"Type: {ldtkWorld.GetType().FullName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating LDtkWorld: {ex.Message}");
            }
        }
    }
}