import * as React from "react";
import {Dispatch, SetStateAction} from "react";
import {CatalogResponseDto, NoteResponseDto} from "../dto/request.response.dto";

/** Глобальный общий стейт с начальной инициализацией некоторых полей */
export class CommonStateStorage {

    /** Используется для передачи идентификатора заметки между компонентами */
    private _commonNumber: number = 0;
    public get commonNumber() {return this._commonNumber};
    public set commonNumber(value: number) {this._commonNumber = value};


    /** CreateComponent: используется для режима "подтверждение/отмена" */
    // TODO: принимает значения 0/1 - переделать в false/true:
    private _createComponentState: number = 0;
    public get createComponentState() {return this._createComponentState};
    public set createComponentState(value: number) {this._createComponentState = value;}


    /** CreateComponent: Используется для хранения JSON string с заметкой при режиме "подтверждение/отмена" */
    private _createComponentString: string = "";
    public get createComponentString() {return this._createComponentString};
    public set createComponentString(value: string) {this._createComponentString = value};


    /** Восстановление начальных значений */
    public init = () => {
        this._createComponentState = 0;
        this._createComponentString = "";
    }
}

/** Глобальный стейт для восстановления компонента после ошибки авторизации */
export class RecoveryStateStorage<T> {

    /** Сохраняется на LoginBoxVisibility(true), восстанавливается в Login.continueLoading */
    private _recoveryString: string = "";
    public get recoveryString() {return this._recoveryString};
    public set recoveryString(value: string) {this._recoveryString = value};


    /** Сохраняется на LoginBoxVisibility(true), восстанавливается в Login.continueLoading */
    private _recoveryStateWrapper?: FunctionComponentStateWrapper<T>;
    public get recoveryStateWrapper(): FunctionComponentStateWrapper<T>|undefined {return this._recoveryStateWrapper};// возможно, undefined стоит убрать
    public set recoveryStateWrapper(value: FunctionComponentStateWrapper<T>) {this._recoveryStateWrapper = value};
}

/** Обёртка, позволяющая загрузчику менять стейт функциональных компонентов приложения */
export class FunctionComponentStateWrapper<T> {
    public mounted: [boolean, React.Dispatch<React.SetStateAction<boolean>>];
    public setData: Dispatch<SetStateAction<T|null>>;

    constructor(mounted: [boolean, React.Dispatch<React.SetStateAction<boolean>>],
                setData: Dispatch<SetStateAction<T|null>>) {
        this.mounted = mounted;
        this.setData = setData;
    }
}

/** Альяс возможных типов для обёртки стейта */
export type StateTypesAlias = NoteResponseDto&CatalogResponseDto;
