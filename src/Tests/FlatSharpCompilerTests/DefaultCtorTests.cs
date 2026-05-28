/*
 * Copyright 2020 James Courtney
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Linq;

namespace FlatSharpTests.Compiler;

public class DefaultCtorTests
{
    [Fact]
    public void DefaultCtorKind_NotSpecified_Table()
    {
        string schema = $"{MetadataHelpers.AllAttributes}namespace Foo; table BaseTable {{ Int:int; }}";

        Assembly asm = FlatSharpCompiler.CompileAndLoadAssembly(schema, new());
        Type baseTableType = asm.GetTypes().Single(x => x.Name == "BaseTable");

        var constructor = baseTableType.GetConstructor(new Type[0]);
        Assert.NotNull(constructor);
        Assert.True(constructor.IsPublic);
        Assert.Null(constructor.GetCustomAttribute<ObsoleteAttribute>());
    }

    [Fact]
    public void DefaultCtorKind_NotSpecified_Struct()
    {
        string schema = $"{MetadataHelpers.AllAttributes}namespace Foo; struct BaseTable {{ Int:int; }}";

        Assembly asm = FlatSharpCompiler.CompileAndLoadAssembly(schema, new());
        Type baseTableType = asm.GetTypes().Single(x => x.Name == "BaseTable");

        var constructor = baseTableType.GetConstructor(new Type[0]);
        Assert.NotNull(constructor);
        Assert.True(constructor.IsPublic);
        Assert.Null(constructor.GetCustomAttribute<ObsoleteAttribute>());
    }

    [Fact]
    public void DefaultCtorKind_NoValue_Table()
    {
        string schema = $"{MetadataHelpers.AllAttributes}namespace Foo; table BaseTable ({MetadataKeys.DefaultConstructorKind}) {{ Int:int; }}";

        Assembly asm = FlatSharpCompiler.CompileAndLoadAssembly(schema, new());
        Type baseTableType = asm.GetTypes().Single(x => x.Name == "BaseTable");

        var constructor = baseTableType.GetConstructor(new Type[0]);
        Assert.NotNull(constructor);
        Assert.True(constructor.IsPublic);
        Assert.Null(constructor.GetCustomAttribute<ObsoleteAttribute>());
    }

    [Fact]
    public void DefaultCtorKind_NoValue_Struct()
    {
        string schema = $"{MetadataHelpers.AllAttributes}namespace Foo; struct BaseTable ({MetadataKeys.DefaultConstructorKind}) {{ Int:int; }}";

        Assembly asm = FlatSharpCompiler.CompileAndLoadAssembly(schema, new());
        Type baseTableType = asm.GetTypes().Single(x => x.Name == "BaseTable");

        var constructor = baseTableType.GetConstructor(new Type[0]);
        Assert.NotNull(constructor);
        Assert.True(constructor.IsPublic);
        Assert.Null(constructor.GetCustomAttribute<ObsoleteAttribute>());
    }

    [Fact]
    public void DefaultCtorKind_None_Struct()
    {
        string schema = $@"
            {MetadataHelpers.AllAttributes}
            namespace Foo; 
            struct InnerStruct ({MetadataKeys.DefaultConstructorKind}:""{DefaultConstructorKind.None}"") {{ Int:int; }}";

        var ex = Assert.Throws<InvalidFbsFileException>(() => FlatSharpCompiler.CompileAndLoadAssembly(schema, new()));
        Assert.Contains("The attribute 'fs_defaultCtor' value None is not valid on ReferenceStruct elements.", ex.Errors[0]);
    }

    [Fact]
    public void DefaultCtorKind_None_Table()
    {
        string schema = $@"
            {MetadataHelpers.AllAttributes}
            namespace Foo; 
            table Table ({MetadataKeys.DefaultConstructorKind}:""{DefaultConstructorKind.None}"") {{ Int:int; }}";

        var asm = FlatSharpCompiler.CompileAndLoadAssembly(schema, new());
        Type baseTableType = asm.GetTypes().Single(x => x.Name == "Table");

        var constructor = baseTableType.GetConstructor(new Type[0]);
        Assert.Null(constructor);
    }

    [Fact]
    public void DefaultCtorKind_Public_Struct()
    {
        string schema = $@"
            {MetadataHelpers.AllAttributes}
            namespace Foo; 
            struct InnerStruct ({MetadataKeys.DefaultConstructorKind}:""{DefaultConstructorKind.Public}"") {{ Int:int; }}";

        var asm = FlatSharpCompiler.CompileAndLoadAssembly(schema, new());
        Type baseTableType = asm.GetTypes().Single(x => x.Name == "InnerStruct");

        var constructor = baseTableType.GetConstructor(new Type[0]);
        Assert.NotNull(constructor);
        Assert.True(constructor.IsPublic);
        Assert.Null(constructor.GetCustomAttribute<ObsoleteAttribute>());
    }

    [Fact]
    public void DefaultCtorKind_Public_Table()
    {
        string schema = $@"
            {MetadataHelpers.AllAttributes}
            namespace Foo; 
            table Table ({MetadataKeys.DefaultConstructorKind}:""{DefaultConstructorKind.Public}"") {{ Int:int; }}";

        var asm = FlatSharpCompiler.CompileAndLoadAssembly(schema, new());
        Type baseTableType = asm.GetTypes().Single(x => x.Name == "Table");

        var constructor = baseTableType.GetConstructor(new Type[0]);
        Assert.NotNull(constructor);
        Assert.True(constructor.IsPublic);
        Assert.Null(constructor.GetCustomAttribute<ObsoleteAttribute>());
    }

    [Fact]
    public void DefaultCtorKind_PublicObsolete_Struct()
    {
        string schema = $@"
            {MetadataHelpers.AllAttributes}
            namespace Foo; 
            struct InnerStruct ({MetadataKeys.DefaultConstructorKind}:""{DefaultConstructorKind.PublicObsolete}"") {{ Int:int; }}";

        var asm = FlatSharpCompiler.CompileAndLoadAssembly(schema, new());
        Type baseTableType = asm.GetTypes().Single(x => x.Name == "InnerStruct");

        var constructor = baseTableType.GetConstructor(new Type[0]);
        Assert.NotNull(constructor);
        Assert.True(constructor.IsPublic);
        Assert.NotNull(constructor.GetCustomAttribute<ObsoleteAttribute>());
    }

    [Fact]
    public void DefaultCtorKind_PublicObsolete_Table()
    {
        string schema = $@"
            {MetadataHelpers.AllAttributes}
            namespace Foo;
            table Table ({MetadataKeys.DefaultConstructorKind}:""{DefaultConstructorKind.PublicObsolete}"") {{ Int:int; }}";

        var asm = FlatSharpCompiler.CompileAndLoadAssembly(schema, new());
        Type baseTableType = asm.GetTypes().Single(x => x.Name == "Table");

        var constructor = baseTableType.GetConstructor(new Type[0]);
        Assert.NotNull(constructor);
        Assert.True(constructor.IsPublic);
        Assert.NotNull(constructor.GetCustomAttribute<ObsoleteAttribute>());
    }

    [Fact]
    public void DefaultCtorKind_Parameterized_Table_NoDefaultCtor()
    {
        string schema = $@"
            {MetadataHelpers.AllAttributes}
            namespace Foo;
            table Table ({MetadataKeys.DefaultConstructorKind}:""{DefaultConstructorKind.Parameterized}"") {{ Int:int; Str:string; }}";

        var asm = FlatSharpCompiler.CompileAndLoadAssembly(schema, new());
        Type tableType = asm.GetTypes().Single(x => x.Name == "Table");

        // No parameterless constructor.
        var defaultCtor = tableType.GetConstructor(new Type[0]);
        Assert.Null(defaultCtor);
    }

    [Fact]
    public void DefaultCtorKind_Parameterized_Table_HasParameterizedCtor()
    {
        string schema = $@"
            {MetadataHelpers.AllAttributes}
            namespace Foo;
            table Table ({MetadataKeys.DefaultConstructorKind}:""{DefaultConstructorKind.Parameterized}"") {{ Int:int; Str:string; }}";

        var asm = FlatSharpCompiler.CompileAndLoadAssembly(schema, new());
        Type tableType = asm.GetTypes().Single(x => x.Name == "Table");

        // One public constructor with parameters for each field.
        var ctors = tableType.GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var paramCtor = ctors.SingleOrDefault(c => c.GetParameters().Length == 2);
        Assert.NotNull(paramCtor);

        var paramNames = paramCtor.GetParameters().Select(p => p.Name).ToArray();
        Assert.Contains("int", paramNames);
        Assert.Contains("str", paramNames);
    }

    [Fact]
    public void DefaultCtorKind_Parameterized_Table_ConstructorAssignsFields()
    {
        string schema = $@"
            {MetadataHelpers.AllAttributes}
            namespace Foo;
            table Table ({MetadataKeys.DefaultConstructorKind}:""{DefaultConstructorKind.Parameterized}"") {{ Int:int; }}";

        var asm = FlatSharpCompiler.CompileAndLoadAssembly(schema, new());
        Type tableType = asm.GetTypes().Single(x => x.Name == "Table");

        var paramCtor = tableType.GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Single(c => c.GetParameters().Length == 1);

        var instance = paramCtor.Invoke(new object[] { 42 });
        var intProp = tableType.GetProperty("Int")!;
        Assert.Equal(42, intProp.GetValue(instance));
    }
}
