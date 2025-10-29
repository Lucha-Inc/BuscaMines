-- Manually validated which bugprone- checks apply to Unreal Engine code.
enable_checks("bugprone-bool-pointer-implicit-conversion")
enable_checks("bugprone-branch-clone")
enable_checks("bugprone-copy-constructor-init")
enable_checks("bugprone-forwarding-reference-overload")
enable_checks("bugprone-implicit-widening-of-multiplication-result")
enable_checks("bugprone-incorrect-roundings")
enable_checks("bugprone-infinite-loop")
enable_checks("bugprone-integer-division")
-- Too many warnings in test module, and we don't have NOLINTBEGIN/NOLINTEND until clang-tidy 14.
-- enable_checks("bugprone-macro-parentheses")
enable_checks("bugprone-macro-repeated-side-effects")
enable_checks("bugprone-misplaced-widening-cast")
enable_checks("bugprone-parent-virtual-call")
enable_checks("bugprone-redundant-branch-condition")
enable_checks("bugprone-signed-char-misuse")
enable_checks("bugprone-sizeof-expression")
enable_checks("bugprone-suspicious-enum-usage")
enable_checks("bugprone-suspicious-semicolon")
enable_checks("bugprone-swapped-arguments")
enable_checks("bugprone-terminating-continue")
enable_checks("bugprone-too-small-loop-variable")
enable_checks("bugprone-undelegated-constructor")
enable_checks("bugprone-unhandled-self-assignment")
enable_checks("bugprone-virtual-near-miss") 

-- Manually validated which performance- checks apply to Unreal Engine code.
enable_checks("performance-for-range-copy")
enable_checks("performance-implicit-conversion-in-loop")
enable_checks("performance-move-constructor-init")
enable_checks("performance-no-automatic-move")
enable_checks("performance-trivially-destructible")
enable_checks("performance-unnecessary-copy-initialization")
enable_checks("performance-unnecessary-value-param")

-- Unreal Engine checks.
enable_checks("unreal-*")

-- Treat certain bugs as errors.
treat_as_errors("bugprone-*")
treat_as_errors("performance-*") 
treat_as_errors("unreal-*")
treat_as_errors("eos-*")

check {
    name = "eos-uninitialized-struct",
    enabled = true,
    description = [[
        Detects uninitialized EOS variables. These are dangerous because if new fields
        are added in later SDK versions, they will be left uninitialized in Shipping builds.

        All EOS structs are required to be initialized like so:
        
        EOS_SomeStruct Val = {};

        With all field assignments following after that.
    ]],
    matcher = [[
        varDecl(hasType(namedDecl(matchesName("^EOS_.+")).bind("eos_type")), hasLocalStorage(), hasDescendant(cxxConstructExpr(argumentCountIs(0)))).bind("variable_declaration")
    ]],
    message = [[
        uninitialized EOS structure will lead to undefined behaviour at runtime; use an empty initializer to force zero initialization
    ]],
    callsite = "variable_declaration"
}

check {
    name = "unreal-shared-class-is-copyable",
    description = [[
        Detects if a class implements TSharedFromThis<>, but doesn't use UE_NONCOPYABLE
        to prevent implicit copies.

        If you don't delete the constructors with UE_NONCOPYABLE, C++ will generate default
        copy and move constructors which will allow broken code like this to compile:

        TSomeClass& A = *SomePtrToSharedClassA;
        TSomeClass B = A; // Implicit copy.
        B.AsShared();     // Crash.
    ]],
    matcher = [[
        cxxRecordDecl(
            isDerivedFrom(
                classTemplateSpecializationDecl(
                    hasName("TSharedFromThis")
                )
            ),
            hasDefinition(),
            has(decl()),
            unless(
                has(
                    cxxConstructorDecl(
                        isCopyConstructor(), 
                        isDeleted(),
                        isExpandedFromMacro("UE_NONCOPYABLE")
                    )
                )
            ),
            unless(
                anyOf(
                    isExpandedFromMacro("DEFINE_LATENT_AUTOMATION_COMMAND"),
                    isExpandedFromMacro("DEFINE_LATENT_AUTOMATION_COMMAND_ONE_PARAMETER"),
                    isExpandedFromMacro("DEFINE_LATENT_AUTOMATION_COMMAND_TWO_PARAMETER"),
                    isExpandedFromMacro("DEFINE_LATENT_AUTOMATION_COMMAND_THREE_PARAMETER"),
                    isExpandedFromMacro("DEFINE_LATENT_AUTOMATION_COMMAND_FOUR_PARAMETER"),
                    isExpandedFromMacro("DEFINE_LATENT_AUTOMATION_COMMAND_FIVE_PARAMETER")
                )
            )
        ).bind("class_definition")
    ]],
    message = [[class inherits from TSharedFromThis<> (either directly or otherwise), but does not delete the copy or constructors. use UE_NONCOPYABLE(ClassName) in the class body to delete the copy and move constructors.]],
    callsite = "class_definition"
}

check {
    name = "unreal-field-not-initialized-in-constructor",
    description = [[
        Detects if a field in a class or struct is not initialized in the 
        constructor's initialization list when at least one member is initialized
        via the initializer list.
    ]],
    matcher = [[
        cxxConstructorDecl(
            unless(isImplicit()),
            unless(isDelegatingConstructor()),
            unless(isDeleted()),
            unless(isDefaulted()),
            hasBody(stmt()),
            unless(ofClass(cxxRecordDecl(isUClass()))),
            unless(ofClass(cxxRecordDecl(isUInterface()))),
            ofClass(cxxRecordDecl(forEach(fieldDecl().bind("declared_field")))),
            forNone(cxxCtorInitializer(forField(fieldDecl(equalsBoundNode("declared_field")).bind("referenced_field"))))
        ).bind("bad_constructor")
    ]],
    message = [[one or more fields will be uninitialized when class or struct is constructed; please add the field to the initializer list.]],
    callsite = "bad_constructor",
    hints = {
        declared_field = "this field must be initialized",
    }
}

check {
    name = "unreal-base-class-not-initialized-in-constructor",
    description = [[
        Detects if a base class of a class or struct does not have it's constructor
        called in the derived class's initialization list.
    ]],
    matcher = [[
        cxxConstructorDecl(
            unless(isImplicit()),
            unless(isDelegatingConstructor()),
            unless(isDeleted()),
            unless(isDefaulted()),
            hasBody(stmt()),
            unless(ofClass(cxxRecordDecl(isUClass()))),
            unless(ofClass(cxxRecordDecl(isUInterface()))),
            ofClass(cxxRecordDecl(hasDirectBase(cxxBaseSpecifier(hasTypeLoc(loc(cxxRecordDecl().bind("base_class")))).bind("missing_constructor_for")))),
            forNone(cxxCtorInitializer(hasTypeLoc(loc(cxxRecordDecl(equalsBoundNode("base_class"))))))
        ).bind("bad_constructor")
    ]],
    message = [[one or more base classes will be uninitialized when class or struct is constructed; please add a base class constructor call to the initializer list.]],
    callsite = "bad_constructor",
    hints = {
        missing_constructor_for = "this base class must be initialized",
    }
}

check {
    name = "unreal-self-referencing-initialization-expression",
    description = [[
        Detects when you write something like this:

        FMyClass(long InSomeField) : SomeField(SomeField){};

        since referencing a field in it's own initializer is undefined.
    ]],
    matcher = [[
        cxxConstructorDecl(
            unless(isImplicit()),
            unless(isDelegatingConstructor()),
            unless(isDeleted()),
            unless(isDefaulted()),
            hasBody(stmt()),
            forEach(cxxCtorInitializer(
                forField(fieldDecl().bind("field")), 
                withInitializer(hasDescendant(memberExpr(
                    hasDeclaration(fieldDecl(equalsBoundNode("field"))),
                    hasObjectExpression(cxxThisExpr())
                )))
            ).bind("bad_initializer"))
        )
    ]],
    message = [[this field initializer references itself.]],
    callsite = "bad_initializer"
}

check {
    name = "unreal-incorrect-fmemory-init",
    enabled = true,
    description = [[
        Detects FMemoryWriter or FMemoryReader instantiations where the persistent parameter is
        not set. The persistence value must be the same for both the reading and writing code to
        avoid data corruption. Unless you're certain that the data will never leave the local memory
        of the machine, you must explicitly set the persistence parameter to true (it defaults to false).
    ]],
    matcher = [[
        varDecl(
            hasType(
                namedDecl(
                    matchesName("FMemory(Reader|Writer)")
                )
            ), 
            hasDescendant(
                cxxConstructExpr(
                    hasArgument(
                        1, 
                        cxxDefaultArgExpr()
                    )
                )
            )
        ).bind("variable_declaration")
    ]],
    message = [[
        FMemoryReader or FMemoryWriter has implicit persistence argument; set it to true unless you're certain that the serialized data will not leave local memory
    ]],
    callsite = "variable_declaration"
}
