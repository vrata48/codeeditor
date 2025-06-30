using CodeEditor.MCP.Models;
using CodeEditor.MCP.Services.CodeStructure;

namespace CodeEditor.MCP.Services;

/// <summary>
/// Main orchestrator for all code structure operations
/// </summary>
public class CodeStructureService : ICodeStructureService
{
    private readonly ICodeAnalysisService _analysisService;
    private readonly ICodeModificationService _modificationService;
    private readonly ICodeQueryService _queryService;
    private readonly ICodeValidationService _validationService;
    private readonly ICodeRefactoringService _refactoringService;
    private readonly IBatchOperationsService _batchOperationsService;
    private readonly ICodeGenerationService _generationService;

    public CodeStructureService(
        ICodeAnalysisService analysisService,
        ICodeModificationService modificationService,
        ICodeQueryService queryService,
        ICodeValidationService validationService,
        ICodeRefactoringService refactoringService,
        IBatchOperationsService batchOperationsService,
        ICodeGenerationService generationService)
    {
        _analysisService = analysisService;
        _modificationService = modificationService;
        _queryService = queryService;
        _validationService = validationService;
        _refactoringService = refactoringService;
        _batchOperationsService = batchOperationsService;
        _generationService = generationService;
    }

    #region Analysis Methods
    public CodeTypeDefinition ParseType(string filePath, string typeName)
        => _analysisService.ParseType(filePath, typeName);

    public List<CodeTypeDefinition> ParseAllTypes(string filePath)
        => _analysisService.ParseAllTypes(filePath);

    public ProjectStructure AnalyzeProject(string projectPath = ".")
        => _analysisService.AnalyzeProject(projectPath);
    #endregion

    #region Querying Methods
    public List<CodeTypeDefinition> FindTypesByName(string pattern)
        => _queryService.FindTypesByName(pattern);

    public List<CodeMethodDefinition> FindMethodsBySignature(string returnType, string name, params string[] parameterTypes)
        => _queryService.FindMethodsBySignature(returnType, name, parameterTypes);

    public List<CodeTypeDefinition> FindTypesWithAttribute(string attributeName)
        => _queryService.FindTypesWithAttribute(attributeName);

    public List<string> FindAllReferences(string typeName, string memberName)
        => _queryService.FindAllReferences(typeName, memberName);

    public List<string> GetChangeImpact(string typeName, string memberName)
        => _queryService.GetChangeImpact(typeName, memberName);

    public List<string> GetDependentFiles(string filePath)
        => _queryService.GetDependentFiles(filePath);

    public List<string> GetUsages(string typeName, string? memberName = null)
        => _queryService.GetUsages(typeName, memberName);
    #endregion

    #region High-Level Modification Operations
    public void ModifyType(CodeTypeDefinition type)
        => _modificationService.ModifyType(type);

    public void RenameSymbol(string oldName, string newName, string? typeName = null)
        => _refactoringService.RenameSymbol(oldName, newName, typeName);
    #endregion

    #region Method Management
    public void AddMethod(string filePath, string typeName, CodeMethodDefinition method)
        => _modificationService.AddMethod(filePath, typeName, method);

    public void RemoveMethod(string filePath, string typeName, string methodName)
        => _modificationService.RemoveMethod(filePath, typeName, methodName);

    public void ReplaceMethod(string filePath, string typeName, string oldMethodName, CodeMethodDefinition newMethod)
        => _modificationService.ReplaceMethod(filePath, typeName, oldMethodName, newMethod);

    public CodeMethodDefinition? GetMethod(string filePath, string typeName, string methodName)
    {
        var type = _analysisService.ParseType(filePath, typeName);
        return type.FindMethod(methodName);
    }

    public string GetMethodBody(string filePath, string typeName, string methodName)
    {
        var method = GetMethod(filePath, typeName, methodName);
        return method?.Body ?? throw new InvalidOperationException($"Method '{methodName}' not found");
    }

    public void UpdateMethodBody(string filePath, string typeName, string methodName, string newBody)
        => _modificationService.UpdateMethodBody(filePath, typeName, methodName, newBody);
    #endregion

    #region Property Management
    public void AddProperty(string filePath, string typeName, CodePropertyDefinition property)
        => _modificationService.AddProperty(filePath, typeName, property);

    public void RemoveProperty(string filePath, string typeName, string propertyName)
        => _modificationService.RemoveProperty(filePath, typeName, propertyName);

    public void ReplaceProperty(string filePath, string typeName, string oldPropertyName, CodePropertyDefinition newProperty)
        => _modificationService.ReplaceProperty(filePath, typeName, oldPropertyName, newProperty);

    public CodePropertyDefinition? GetProperty(string filePath, string typeName, string propertyName)
    {
        var type = _analysisService.ParseType(filePath, typeName);
        return type.Members.Properties.FirstOrDefault(p => p.Name == propertyName);
    }
    #endregion

    #region Field Management
    public void AddField(string filePath, string typeName, CodeFieldDefinition field)
        => _modificationService.AddField(filePath, typeName, field);

    public void RemoveField(string filePath, string typeName, string fieldName)
        => _modificationService.RemoveField(filePath, typeName, fieldName);

    public void ReplaceField(string filePath, string typeName, string oldFieldName, CodeFieldDefinition newField)
        => _modificationService.ReplaceField(filePath, typeName, oldFieldName, newField);
    #endregion

    #region Interface Management
    public void CreateInterface(string filePath, string interfaceName, CodeTypeDefinition interfaceDefinition)
        => _modificationService.CreateType(filePath, interfaceDefinition);

    public void AddMethodToInterface(string filePath, string interfaceName, CodeMethodDefinition method)
        => _modificationService.AddMethod(filePath, interfaceName, method);

    public void RemoveMethodFromInterface(string filePath, string interfaceName, string methodName)
        => _modificationService.RemoveMethod(filePath, interfaceName, methodName);

    public void AddPropertyToInterface(string filePath, string interfaceName, CodePropertyDefinition property)
        => _modificationService.AddProperty(filePath, interfaceName, property);
    #endregion

    #region Type Management
    public void CreateType(string filePath, CodeTypeDefinition type)
        => _modificationService.CreateType(filePath, type);

    public void RemoveType(string filePath, string typeName)
        => _modificationService.RemoveType(filePath, typeName);

    public void AddInterface(string filePath, string typeName, string interfaceName)
        => _modificationService.AddInterface(filePath, typeName, interfaceName);

    public void RemoveInterface(string filePath, string typeName, string interfaceName)
        => _modificationService.RemoveInterface(filePath, typeName, interfaceName);

    public void ChangeBaseClass(string filePath, string typeName, string? newBaseClass)
        => _modificationService.ChangeBaseClass(filePath, typeName, newBaseClass);
    #endregion

    #region Convenience Methods
    public void AddPublicMethod(string filePath, string typeName, string methodName, string returnType, 
        string body, params (string type, string name)[] parameters)
    {
        var method = new CodeMethodDefinition
        {
            Name = methodName,
            ReturnType = returnType,
            Visibility = "public",
            Body = body,
            Parameters = parameters.Select(p => new CodeParameterDefinition 
            { 
                Type = p.type, 
                Name = p.name 
            }).ToList()
        };
        
        AddMethod(filePath, typeName, method);
    }

    public void AddPrivateMethod(string filePath, string typeName, string methodName, string returnType, 
        string body, params (string type, string name)[] parameters)
    {
        var method = new CodeMethodDefinition
        {
            Name = methodName,
            ReturnType = returnType,
            Visibility = "private",
            Body = body,
            Parameters = parameters.Select(p => new CodeParameterDefinition 
            { 
                Type = p.type, 
                Name = p.name 
            }).ToList()
        };
        
        AddMethod(filePath, typeName, method);
    }

    public void AddPublicProperty(string filePath, string typeName, string propertyName, string type, 
        bool hasGetter = true, bool hasSetter = true)
    {
        var property = new CodePropertyDefinition
        {
            Name = propertyName,
            Type = type,
            Visibility = "public",
            HasGetter = hasGetter,
            HasSetter = hasSetter
        };
        
        AddProperty(filePath, typeName, property);
    }

    public void AddPrivateField(string filePath, string typeName, string fieldName, string type, 
        string? defaultValue = null)
    {
        var field = new CodeFieldDefinition
        {
            Name = fieldName,
            Type = type,
            Visibility = "private",
            DefaultValue = defaultValue
        };
        
        AddField(filePath, typeName, field);
    }
    #endregion

    #region Batch Operations
    public void AddMethods(string filePath, string typeName, IEnumerable<CodeMethodDefinition> methods)
        => _batchOperationsService.AddMethods(filePath, typeName, methods);

    public void AddProperties(string filePath, string typeName, IEnumerable<CodePropertyDefinition> properties)
        => _batchOperationsService.AddProperties(filePath, typeName, properties);

    public void RemoveMethods(string filePath, string typeName, IEnumerable<string> methodNames)
        => _batchOperationsService.RemoveMethods(filePath, typeName, methodNames);
    #endregion

    #region Validation
    public bool TypeExists(string filePath, string typeName)
        => _validationService.TypeExists(filePath, typeName);

    public bool MethodExists(string filePath, string typeName, string methodName)
        => _validationService.MethodExists(filePath, typeName, methodName);

    public bool PropertyExists(string filePath, string typeName, string propertyName)
        => _validationService.PropertyExists(filePath, typeName, propertyName);

    public List<string> ValidateModification(string filePath, string typeName, string operation)
        => _validationService.ValidateModification(filePath, typeName, operation);
    #endregion

    #region Code Generation
    public string GenerateCode(CodeTypeDefinition type)
        => _generationService.GenerateCode(type);

    public void RegenerateFile(string filePath, List<CodeTypeDefinition> types)
        => _modificationService.RegenerateFile(filePath, types);
    #endregion
}
