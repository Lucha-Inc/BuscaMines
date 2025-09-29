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

    // Inicializar desde un FCacoUserStruct
    void Init(const FCacoUserStruct& InStruct);

    // Factory para crear directamente un UCacoUser desde struct
    UFUNCTION(BlueprintCallable, Category = "CacoUser System")
    static UCacoUser* CreateUserFromStruct(UObject* Outer, const FCacoUserStruct& InStruct);

    UFUNCTION(BlueprintPure, Category = "CacoUser System")
    FString GetUserId() const;

    UFUNCTION(BlueprintPure, Category = "CacoUser System")
    FString GetDisplayName() const;

protected:

    UPROPERTY(BlueprintReadOnly, Category = "CacoUser System")
    FString UserId;

    UPROPERTY(BlueprintReadOnly, Category = "CacoUser System")
    FString DisplayName;
};










