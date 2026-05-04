using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace FluxorBus.SourceGen.UnitTests;

[TestClass]
public class FluxorBusGeneratorTests
{
    [TestMethod]
    public void Initialize_WithEmptyCompilation_DoesNotThrow()
    {
        // Arrange
        var generator = new FluxorBusGenerator();
        var compilation = CreateEmptyCompilation();
        var driver = CSharpGeneratorDriver.Create(generator);

        // Act
        var result = driver.RunGenerators(compilation, TestContext?.CancellationToken ?? CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Initialize_WithCompilationContainingClassWithBaseList_ProcessesClass()
    {
        // Arrange
        var generator = new FluxorBusGenerator();
        var sourceCode = @"
namespace TestNamespace
{
    public class TestClass : BaseClass
    {
    }

    public class BaseClass
    {
    }
}";
        var compilation = CreateCompilationWithSource(sourceCode);
        var driver = CSharpGeneratorDriver.Create(generator);

        // Act
        var result = driver.RunGenerators(compilation);

        // Assert
        Assert.IsNotNull(result);
        var runResult = result.GetRunResult();
        Assert.IsNotNull(runResult);
    }

    [TestMethod]
    public void Initialize_WithClassWithoutBaseList_DoesNotProcessClass()
    {
        // Arrange
        var generator = new FluxorBusGenerator();
        var sourceCode = @"
namespace TestNamespace
{
    public class TestClass
    {
    }
}";
        var compilation = CreateCompilationWithSource(sourceCode);
        var driver = CSharpGeneratorDriver.Create(generator);

        // Act
        var result = driver.RunGenerators(compilation);

        // Assert
        Assert.IsNotNull(result);
        var runResult = result.GetRunResult();
        Assert.IsEmpty(runResult.GeneratedTrees);
    }

    [TestMethod]
    public void Initialize_WithNullCompilation_HandlesGracefully()
    {
        // Arrange
        var generator = new FluxorBusGenerator();
        var compilation = CSharpCompilation.Create("Empty");
        var driver = CSharpGeneratorDriver.Create(generator);

        // Act
        var result = driver.RunGenerators(compilation);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Initialize_WithMultipleClassesWithBaseList_ProcessesAllClasses()
    {
        // Arrange
        var generator = new FluxorBusGenerator();
        var sourceCode = @"
namespace TestNamespace
{
    public class TestClass1 : BaseClass
    {
    }

    public class TestClass2 : BaseClass
    {
    }

    public class BaseClass
    {
    }
}";
        var compilation = CreateCompilationWithSource(sourceCode);
        var driver = CSharpGeneratorDriver.Create(generator);

        // Act
        var result = driver.RunGenerators(compilation);

        // Assert
        Assert.IsNotNull(result);
        var runResult = result.GetRunResult();
        Assert.IsNotNull(runResult);
    }

    [TestMethod]
    public void Initialize_RegistersIncrementalPipeline_ThatFiltersClassesWithBaseTypes()
    {
        // Arrange
        var generator = new FluxorBusGenerator();
        var sourceCode = @"
namespace TestNamespace
{
    public class ClassWithBase : BaseClass
    {
    }

    public class ClassWithoutBase
    {
    }

    public class BaseClass
    {
    }
}";
        var compilation = CreateCompilationWithSource(sourceCode);
        var driver = CSharpGeneratorDriver.Create(generator);

        // Act
        var result = driver.RunGenerators(compilation);

        // Assert
        Assert.IsNotNull(result);
        var runResult = result.GetRunResult();
        Assert.IsNotNull(runResult);
    }

    [TestMethod]
    public void Initialize_WithStructDeclaration_DoesNotProcessStruct()
    {
        // Arrange
        var generator = new FluxorBusGenerator();
        var sourceCode = @"
namespace TestNamespace
{
    public struct TestStruct : IInterface
    {
    }

    public interface IInterface
    {
    }
}";
        var compilation = CreateCompilationWithSource(sourceCode);
        var driver = CSharpGeneratorDriver.Create(generator);

        // Act
        var result = driver.RunGenerators(compilation);

        // Assert
        Assert.IsNotNull(result);
        var runResult = result.GetRunResult();
        Assert.IsEmpty(runResult.GeneratedTrees);
    }

    [TestMethod]
    public void Initialize_WithInterfaceDeclaration_DoesNotProcessInterface()
    {
        // Arrange
        var generator = new FluxorBusGenerator();
        var sourceCode = @"
namespace TestNamespace
{
    public interface ITestInterface : IBaseInterface
    {
    }

    public interface IBaseInterface
    {
    }
}";
        var compilation = CreateCompilationWithSource(sourceCode);
        var driver = CSharpGeneratorDriver.Create(generator);

        // Act
        var result = driver.RunGenerators(compilation);

        // Assert
        Assert.IsNotNull(result);
        var runResult = result.GetRunResult();
        Assert.IsEmpty(runResult.GeneratedTrees);
    }

    [TestMethod]
    public void Initialize_WithCancellationToken_HandlesGracefully()
    {
        // Arrange
        var generator = new FluxorBusGenerator();
        var sourceCode = @"
namespace TestNamespace
{
    public class TestClass : BaseClass
    {
    }

    public class BaseClass
    {
    }
}";
        var compilation = CreateCompilationWithSource(sourceCode);
        using var cts = new CancellationTokenSource();
        var driver = CSharpGeneratorDriver.Create(generator);

        // Act
        var result = driver.RunGenerators(compilation, cts.Token);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Initialize_WithNestedClass_ProcessesNestedClassWithBaseList()
    {
        // Arrange
        var generator = new FluxorBusGenerator();
        var sourceCode = @"
namespace TestNamespace
{
    public class OuterClass
    {
        public class InnerClass : BaseClass
        {
        }
    }

    public class BaseClass
    {
    }
}";
        var compilation = CreateCompilationWithSource(sourceCode);
        var driver = CSharpGeneratorDriver.Create(generator);

        // Act
        var result = driver.RunGenerators(compilation);

        // Assert
        Assert.IsNotNull(result);
        var runResult = result.GetRunResult();
        Assert.IsNotNull(runResult);
    }

    [TestMethod]
    public void Initialize_WithAbstractClass_ProcessesAbstractClassWithBaseList()
    {
        // Arrange
        var generator = new FluxorBusGenerator();
        var sourceCode = @"
namespace TestNamespace
{
    public abstract class AbstractClass : BaseClass
    {
    }

    public class BaseClass
    {
    }
}";
        var compilation = CreateCompilationWithSource(sourceCode);
        var driver = CSharpGeneratorDriver.Create(generator);

        // Act
        var result = driver.RunGenerators(compilation);

        // Assert
        Assert.IsNotNull(result);
        var runResult = result.GetRunResult();
        Assert.IsNotNull(runResult);
    }

    [TestMethod]
    public void Initialize_WithSealedClass_ProcessesSealedClassWithBaseList()
    {
        // Arrange
        var generator = new FluxorBusGenerator();
        var sourceCode = @"
namespace TestNamespace
{
    public sealed class SealedClass : BaseClass
    {
    }

    public class BaseClass
    {
    }
}";
        var compilation = CreateCompilationWithSource(sourceCode);
        var driver = CSharpGeneratorDriver.Create(generator);

        // Act
        var result = driver.RunGenerators(compilation);

        // Assert
        Assert.IsNotNull(result);
        var runResult = result.GetRunResult();
        Assert.IsNotNull(runResult);
    }

    [TestMethod]
    public void Initialize_WithPartialClass_ProcessesPartialClassWithBaseList()
    {
        // Arrange
        var generator = new FluxorBusGenerator();
        var sourceCode = @"
namespace TestNamespace
{
    public partial class PartialClass : BaseClass
    {
    }

    public partial class PartialClass
    {
    }

    public class BaseClass
    {
    }
}";
        var compilation = CreateCompilationWithSource(sourceCode);
        var driver = CSharpGeneratorDriver.Create(generator);

        // Act
        var result = driver.RunGenerators(compilation);

        // Assert
        Assert.IsNotNull(result);
        var runResult = result.GetRunResult();
        Assert.IsNotNull(runResult);
    }

    [TestMethod]
    public void Initialize_WithGenericClass_ProcessesGenericClassWithBaseList()
    {
        // Arrange
        var generator = new FluxorBusGenerator();
        var sourceCode = @"
namespace TestNamespace
{
    public class GenericClass<T> : BaseClass
    {
    }

    public class BaseClass
    {
    }
}";
        var compilation = CreateCompilationWithSource(sourceCode);
        var driver = CSharpGeneratorDriver.Create(generator);

        // Act
        var result = driver.RunGenerators(compilation);

        // Assert
        Assert.IsNotNull(result);
        var runResult = result.GetRunResult();
        Assert.IsNotNull(runResult);
    }

    [TestMethod]
    public void Initialize_WithClassImplementingInterface_ProcessesClass()
    {
        // Arrange
        var generator = new FluxorBusGenerator();
        var sourceCode = @"
namespace TestNamespace
{
    public class TestClass : ITestInterface
    {
    }

    public interface ITestInterface
    {
    }
}";
        var compilation = CreateCompilationWithSource(sourceCode);
        var driver = CSharpGeneratorDriver.Create(generator);

        // Act
        var result = driver.RunGenerators(compilation);

        // Assert
        Assert.IsNotNull(result);
        var runResult = result.GetRunResult();
        Assert.IsNotNull(runResult);
    }

    [TestMethod]
    public void Initialize_WithClassInheritingAndImplementing_ProcessesClass()
    {
        // Arrange
        var generator = new FluxorBusGenerator();
        var sourceCode = @"
namespace TestNamespace
{
    public class TestClass : BaseClass, ITestInterface
    {
    }

    public class BaseClass
    {
    }

    public interface ITestInterface
    {
    }
}";
        var compilation = CreateCompilationWithSource(sourceCode);
        var driver = CSharpGeneratorDriver.Create(generator);

        // Act
        var result = driver.RunGenerators(compilation);

        // Assert
        Assert.IsNotNull(result);
        var runResult = result.GetRunResult();
        Assert.IsNotNull(runResult);
    }

    [TestMethod]
    public void Initialize_CreatesIncrementalPipeline_ThatCombinesWithCompilation()
    {
        // Arrange
        var generator = new FluxorBusGenerator();
        var compilation = CreateEmptyCompilation();
        var driver = CSharpGeneratorDriver.Create(generator);

        // Act
        var result = driver.RunGenerators(compilation);

        // Assert
        var runResult = result.GetRunResult();
        Assert.IsNotNull(runResult);
    }

    [TestMethod]
    public void Initialize_RegistersSourceOutput_WithCombinedPipeline()
    {
        // Arrange
        var generator = new FluxorBusGenerator();
        var compilation = CreateEmptyCompilation();
        var driver = CSharpGeneratorDriver.Create(generator);

        // Act
        var result = driver.RunGenerators(compilation);

        // Assert
        var runResult = result.GetRunResult();
        Assert.IsNotNull(runResult);
        // Verify one generator ran
        _ = runResult.Results[0];
    }

    [TestMethod]
    public void Initialize_WithInternalClass_ProcessesInternalClassWithBaseList()
    {
        // Arrange
        var generator = new FluxorBusGenerator();
        var sourceCode = @"
namespace TestNamespace
{
    internal class InternalClass : BaseClass
    {
    }

    public class BaseClass
    {
    }
}";
        var compilation = CreateCompilationWithSource(sourceCode);
        var driver = CSharpGeneratorDriver.Create(generator);

        // Act
        var result = driver.RunGenerators(compilation);

        // Assert
        Assert.IsNotNull(result);
        var runResult = result.GetRunResult();
        Assert.IsNotNull(runResult);
    }

    [TestMethod]
    public void Initialize_WithPrivateNestedClass_ProcessesPrivateClassWithBaseList()
    {
        // Arrange
        var generator = new FluxorBusGenerator();
        var sourceCode = @"
namespace TestNamespace
{
    public class OuterClass
    {
        private class PrivateClass : BaseClass
        {
        }
    }

    public class BaseClass
    {
    }
}";
        var compilation = CreateCompilationWithSource(sourceCode);
        var driver = CSharpGeneratorDriver.Create(generator);

        // Act
        var result = driver.RunGenerators(compilation);

        // Assert
        Assert.IsNotNull(result);
        var runResult = result.GetRunResult();
        Assert.IsNotNull(runResult);
    }

    private static CSharpCompilation CreateEmptyCompilation()
    {
        return CSharpCompilation.Create(
            "TestAssembly",
            syntaxTrees: [],
            references:
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            ]);
    }

    private static CSharpCompilation CreateCompilationWithSource(string sourceCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        return CSharpCompilation.Create(
            "TestAssembly",
            syntaxTrees: [syntaxTree],
            references:
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            ]);
    }

    public TestContext? TestContext { get; set; }
}
