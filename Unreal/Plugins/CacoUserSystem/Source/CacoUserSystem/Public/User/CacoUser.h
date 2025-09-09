#pragma once

#include "CoreMinimal.h"
#include "Login/LoginProviderType.h"
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
    ELoginProviderType Provider; // EOS, Guest, Google, etc.

    FCacoUser() {}
    FCacoUser(const FString& InUserId, const FString& InDisplayName, const ELoginProviderType InProvider)
        : UserId(InUserId), DisplayName(InDisplayName), Provider(InProvider) {
    }
};

