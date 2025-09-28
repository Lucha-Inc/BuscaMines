#pragma once


#include "CoreMinimal.h"
#include "GameFramework/SaveGame.h"
#include "User/CacoUserStruct.h"
#include "SaveGame/SaveGameMaster.h"
#include "SaveGame_LocalUsers.generated.h"

UCLASS(BlueprintType)
class CACOUSERSYSTEM_API USaveGame_LocalUsers : public USaveGameMaster {
    GENERATED_BODY()

public:

    UPROPERTY(BlueprintReadWrite)
    TMap<int32, FCacoUserStruct> LocalUsers;

};

