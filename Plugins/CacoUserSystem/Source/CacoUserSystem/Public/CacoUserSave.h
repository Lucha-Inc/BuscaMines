#pragma once


#include "CoreMinimal.h"
#include "GameFramework/SaveGame.h"
#include "LoginProvider.h"
#include "CacoUserSubsystem.h"
#include "CacoUserSave.generated.h"

UCLASS(Blueprintable)
class CACOUSERSYSTEM_API UCacoUserSave : public USaveGame
{
    GENERATED_BODY()

public:

    UPROPERTY(BlueprintReadWrite)
    TArray<FCacoUser> LocalUsers;

    UPROPERTY(BlueprintReadWrite)
    FCacoUser LastLoggedInUser;

    UPROPERTY(BlueprintReadWrite)
    ELoginProvider LastLoginProvider;

};



