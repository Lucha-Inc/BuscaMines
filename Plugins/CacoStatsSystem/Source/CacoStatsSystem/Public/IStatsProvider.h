#pragma once

#include "CoreMinimal.h"

class IStatsProvider
{
public:
    virtual ~IStatsProvider() = default;

    // Devuelve un mapa clave-valor con estadísticas
    virtual TMap<FString, float> GetStats() const = 0;
};
