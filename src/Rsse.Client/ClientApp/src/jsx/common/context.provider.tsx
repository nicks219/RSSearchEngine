import {createContext} from "react";
import {CommonStateStorage, StateTypesAlias} from "./state.wrappers";

// TODO: попробуй разделить recovery context и common context:
export const CommonContext = createContext(new CommonStateStorage<StateTypesAlias>());
export const CommonContextProvider = CommonContext.Provider;

