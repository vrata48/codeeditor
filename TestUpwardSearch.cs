using System;
using System.IO;
using CodeEditor.MCP.Services;

// Simple test to verify the upward search functionality
namespace CodeEditor.MCP.TestConsole
{
    class Program
    {
        static void Main()
        {
            // Create temporary directories
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var subDir = Path.Combine(tempDir, "subfolder");
            
            try
            {
                Directory.CreateDirectory(subDir);
                
                // Create .gitignore in parent directory
                File.WriteAllText(Path.Combine(tempDir, ".gitignore"), "*.log\ntemp/");
                
                Console.WriteLine($"Created temp directory: {tempDir}");
                Console.WriteLine($"Created sub directory: {subDir}");
                Console.WriteLine($"Created .gitignore in: {tempDir}");
                
                // Create PathService from subdirectory
                var pathService = new PathService(subDir);
                
                // Test if patterns from parent .gitignore work
                Console.WriteLine($"Should ignore 'test.log': {pathService.ShouldIgnore("test.log")}");
                Console.WriteLine($"Should ignore 'temp/': {pathService.ShouldIgnore("temp/")}");
                Console.WriteLine($"Should NOT ignore 'test.txt': {!pathService.ShouldIgnore("test.txt")}");
                
                Console.WriteLine("Test completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
    }
}
