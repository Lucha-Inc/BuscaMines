#pragma once

#include "CoreMinimal.h"
#include "LoginProvider.h"
#include "CacoUser.generated.h"


// Estructura para representar un usuario local
USTRUCT(BlueprintType)
struct CACOUSERSYSTEM_API FCacoUser
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly)
    FString UserId;

    UPROPERTY(BlueprintReadOnly)
    FString DisplayName;

    UPROPERTY(BlueprintReadOnly)
    ELoginProvider Provider; // EOS, Guest, Google, etc.

    FCacoUser() {}
    FCacoUser(const FString& InUserId, const FString& InDisplayName, const ELoginProvider InProvider)
        : UserId(InUserId), DisplayName(InDisplayName), Provider(InProvider) {
    }
};

