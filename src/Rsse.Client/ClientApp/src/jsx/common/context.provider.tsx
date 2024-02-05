import {createContext} from "react";
import {CommonStateStorage} from "./state.wrappers";
import {CatalogResponseDto, NoteResponseDto} from "../dto/request.response.dto";

// TODO: попробуй разделить recovery context и common context:
export const CommonContext = createContext(new CommonStateStorage<NoteResponseDto|CatalogResponseDto>());
export const CommonContextProvider = CommonContext.Provider;
