#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "CacoUserStruct.h"
#include "CacoUser.generated.h"

UCLASS(BlueprintType)
class CACOUSERSYSTEM_API UCacoUser : public UObject {
    GENERATED_BODY()

public:

    // Constructor por defecto
    UCacoUser();

//    void Init2(const FCacoUserStruct& InStruct);
    void Init2();

    // Inicializar desde un FCacoUserStruct
//    void Init(const FCacoUserStruct& InStruct);

    // Factory para crear directamente un UCacoUser desde struct
//    UFUNCTION(BlueprintCallable, Category = "User System")
//    static UCacoUser* CreateFromStruct(UObject* Outer, const FCacoUserStruct& InStruct);

    // Acceso directo a UserId
//    UFUNCTION(BlueprintPure, Category = "User System")
//    FString GetUserId() const { return UserId; }

    // Acceso directo a DisplayName
//    UFUNCTION(BlueprintPure, Category = "User System")
//    FString GetDisplayName() const { return DisplayName; }

protected:

    UPROPERTY(BlueprintReadOnly, Category = "CacoUser System")
    FString UserId;

    UPROPERTY(BlueprintReadOnly, Category = "CacoUser System")
    FString DisplayName;
};










