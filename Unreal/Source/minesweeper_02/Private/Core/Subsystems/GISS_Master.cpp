// Fill out your copyright notice in the Description page of Project Settings.


#include "Core/Subsystems/GISS_Master.h"

void UGISS_Master::Initialize(FSubsystemCollectionBase& Collection) {
    Super::Initialize(Collection);

    BP_OnInitialize();
}

void UGISS_Master::Deinitialize() {
    BP_OnDeinitialize();

    Super::Deinitialize();
}