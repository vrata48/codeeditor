using CodeEditor.MCP.Models;
using System.IO.Abstractions;

namespace CodeEditor.MCP.Services.CodeStructure;

/// <summary>
/// Service responsible for validating code structure operations
/// </summary>
public class CodeValidationService : ICodeValidationService
{
    private readonly IFileSystem _fileSystem;
    private readonly IPathService _pathService;
    private readonly ICodeAnalysisService _codeAnalysis;

    public CodeValidationService(
        IFileSystem fileSystem,
        IPathService pathService,
        ICodeAnalysisService codeAnalysis)
    {
        _fileSystem = fileSystem;
        _pathService = pathService;
        _codeAnalysis = codeAnalysis;
    }

    public bool TypeExists(string filePath, string typeName)
    {
        try
        {
            var fullPath = _pathService.GetFullPath(filePath);
            if (!_fileSystem.File.Exists(fullPath))
                return false;

            _codeAnalysis.ParseType(filePath, typeName);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    public bool MethodExists(string filePath, string typeName, string methodName)
    {
        try
        {
            var type = _codeAnalysis.ParseType(filePath, typeName);
            return type.Members.Methods.Any(m => m.Name == methodName);
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    public bool PropertyExists(string filePath, string typeName, string propertyName)
    {
        try
        {
            var type = _codeAnalysis.ParseType(filePath, typeName);
            return type.Members.Properties.Any(p => p.Name == propertyName);
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    public List<string> ValidateModification(string filePath, string typeName, string operation)
    {
        var validationErrors = new List<string>();
        
        try
        {
            var fullPath = _pathService.GetFullPath(filePath);
            
            // Check if file exists
            if (!_fileSystem.File.Exists(fullPath))
            {
                validationErrors.Add($"File '{filePath}' does not exist");
                return validationErrors;
            }
            
            // Check if type exists
            if (!TypeExists(filePath, typeName))
            {
                validationErrors.Add($"Type '{typeName}' does not exist in file '{filePath}'");
                return validationErrors;
            }
            
            var type = _codeAnalysis.ParseType(filePath, typeName);
            
            // Validate based on operation type
            switch (operation.ToLower())
            {
                case "add_method":
                    ValidateCanAddMember(type, validationErrors);
                    break;
                case "remove_method":
                    ValidateCanRemoveMember(type, validationErrors);
                    break;
                case "modify_method":
                    ValidateCanModifyMember(type, validationErrors);
                    break;
                case "add_property":
                    ValidateCanAddMember(type, validationErrors);
                    break;
                case "remove_property":
                    ValidateCanRemoveMember(type, validationErrors);
                    break;
                case "add_field":
                    ValidateCanAddMember(type, validationErrors);
                    break;
                case "remove_field":
                    ValidateCanRemoveMember(type, validationErrors);
                    break;
                default:
                    validationErrors.Add($"Unknown operation: {operation}");
                    break;
            }
        }
        catch (Exception ex)
        {
            validationErrors.Add($"Validation failed: {ex.Message}");
        }
        
        return validationErrors;
    }

    private void ValidateCanAddMember(CodeTypeDefinition type, List<string> errors)
    {
        if (type.Kind == CodeTypeKind.Interface)
        {
            // Interface-specific validations
            // Interfaces can only have certain types of members
        }
        else if (type.Kind == CodeTypeKind.Enum)
        {
            errors.Add("Cannot add members to enum types");
        }
    }

    private void ValidateCanRemoveMember(CodeTypeDefinition type, List<string> errors)
    {
        if (type.Kind == CodeTypeKind.Interface)
        {
            // Check if removing this member would break inheritance contracts
            // This would require more sophisticated analysis
        }
    }

    private void ValidateCanModifyMember(CodeTypeDefinition type, List<string> errors)
    {
        if (type.Kind == CodeTypeKind.Interface)
        {
            // Interface members have restrictions on modification
        }
    }
}
