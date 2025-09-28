#pragma once

#include "CoreMinimal.h"
#include "CacoUserStruct.generated.h"


// Estructura para representar un usuario local
USTRUCT(BlueprintType)
struct CACOUSERSYSTEM_API FCacoUserStruct
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly)
    FString UserId;

    UPROPERTY(BlueprintReadOnly)
    FString DisplayName;

    FCacoUserStruct() {}
    FCacoUserStruct(const FString& InUserId, const FString& InDisplayName)
        : UserId(InUserId), DisplayName(InDisplayName) {
    }
};

