#pragma once


#include "CoreMinimal.h"
#include "GameFramework/SaveGame.h"
#include "User/CacoUserStruct.h"
#include "CacoUserSave.generated.h"

enum class ELoginProviderType : uint8;

UCLASS(Blueprintable)
class CACOUSERSYSTEM_API UCacoUserSave : public USaveGame
{
    GENERATED_BODY()

public:

    UPROPERTY(BlueprintReadWrite)
    TMap<int32, FCacoUserStruct> LocalUsers;

    UPROPERTY(BlueprintReadWrite)
    int32 LastLoggedInUserIndex;

};



