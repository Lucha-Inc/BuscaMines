// CacoUserSubsystem.cpp
#include "CacoUserSubsystem.h"
#include "Kismet/GameplayStatics.h"
#include "Logging/LogMacros.h"
#include "User/CacoUserSettings.h"
#include "SaveGame/SaveGame_LastLoggedInUserIndex.h"

DEFINE_LOG_CATEGORY_STATIC(LogCacoUserSubsystem, Log, All);





void UCacoUserSubsystem::Initialize(FSubsystemCollectionBase& Collection)
{
    Super::Initialize(Collection);

    CachedSettings = GetDefault<UCacoUserSettings>();
}

void UCacoUserSubsystem::Deinitialize()
{
    Super::Deinitialize();
}

bool UCacoUserSubsystem::Prueba1(int32 val) {


    // Crear instancia
    USaveGame_LastLoggedInUserIndex* SaveInstance = NewObject<USaveGame_LastLoggedInUserIndex>();

    // Asignar valor
    SaveInstance->LastLoggedInUserIndex = val;

    // Guardar usando la función Save() de la clase base
    bool bSaved = SaveInstance->Save();

    if (bSaved) {
        UE_LOG(LogTemp, Log, TEXT("SaveGame guardado correctamente!"));
    } else {
        UE_LOG(LogTemp, Warning, TEXT("Error al guardar SaveGame."));
    }

    return bSaved;
}

bool UCacoUserSubsystem::Prueba2() {
    // Cargar usando la función estática Load() de la clase base
    USaveGame_LastLoggedInUserIndex* LoadedInstance =
        Cast<USaveGame_LastLoggedInUserIndex>(USaveGameMaster::Load(USaveGame_LastLoggedInUserIndex::StaticClass()));

    if (LoadedInstance) {
        int32 LastIndex = LoadedInstance->LastLoggedInUserIndex;
        UE_LOG(LogTemp, Log, TEXT("Último usuario logueado: %d"), LastIndex);
        return true;
    } else {
        UE_LOG(LogTemp, Warning, TEXT("No se encontró un SaveGame previo."));
    }
    return false;
}

int32 UCacoUserSubsystem::GetLastLoggedInUserIndex() const
{
    return 0;
}

TMap<int32, FCacoUserStruct>& UCacoUserSubsystem::GetLocalUsers() const
{
    static TMap<int32, FCacoUserStruct> EmptyArray;
    return EmptyArray;
}









