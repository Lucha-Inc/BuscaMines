#pragma once

#include "CoreMinimal.h"
#include "LoginProvider.generated.h"

UENUM(BlueprintType)
enum class ELoginProvider : uint8
{
    Guest       UMETA(DisplayName = "Guest"),
    Google      UMETA(DisplayName = "Google"),
    EOS         UMETA(DisplayName = "Epic Online Services"),
    Microsoft   UMETA(DisplayName = "Microsoft"),
    Facebook    UMETA(DisplayName = "Facebook"),
    Steam       UMETA(DisplayName = "Steam")
};

