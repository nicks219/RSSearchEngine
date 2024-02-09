import {createContext} from "react";
import {CommonStateStorage, RecoveryStateStorage, StateTypesAlias} from "./state.wrappers";

/** Предоставление доступа к расшаренному между компонентами состоянию */
export const CommonContext = createContext(new CommonStateStorage());
export const CommonContextProvider = CommonContext.Provider;


/** Предоставление доступа к состоянию, используемому для восстановления после сбоя авторизации */
export const RecoveryContext = createContext(new RecoveryStateStorage<StateTypesAlias>());
export const RecoveryContextProvider = RecoveryContext.Provider;
