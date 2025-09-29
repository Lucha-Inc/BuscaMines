#pragma once


#include "CoreMinimal.h"
#include "GameFramework/SaveGame.h"
#include "SaveGame/SaveGameMaster.h"
#include "SaveGame_LastLoggedInUserIndex.generated.h"

UCLASS(BlueprintType)
class CACOUSERSYSTEM_API USaveGame_LastLoggedInUserIndex : public USaveGameMaster {
    GENERATED_BODY()

public:

    USaveGame_LastLoggedInUserIndex() : LastLoggedInUserIndex(-1) {}

    UPROPERTY(BlueprintReadWrite, Category = "User System")
    int32 LastLoggedInUserIndex = -1;

};










