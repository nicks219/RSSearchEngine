import * as React from "react";
import {Dispatch, SetStateAction} from "react";

/** Глобальный стейт с начальной инициализацией некоторых полей */
export class CommonStateStorage<T> {

    /** I: CatalogComponent: работа с дампами; II: CreateComponent: режима "подтверждение/отмена" */
    private _commonState: number = 0;
    public get commonState() {return this._commonState};
    public set commonState(value: number) {this._commonState = value;}


    /** I: Выставляется в Login.SetVisible<T>; II: Используется для хранения JSON string с заметкой в Create в режиме "подтверждение/отмена" */
    private _commonString: string = "";
    public get commonString() {return this._commonString};
    public set commonString(value: string) {this._commonString = value};


    /** Используется для передачи идентификатора заметки между компонентами */
    private _commonNumber: number = 0;
    public get commonNumber() {return this._commonNumber};
    public set commonNumber(value: number) {this._commonNumber = value};


    /** Используется для продолжения загрузки: сохраняется на LoginBoxVisibility(true), восстанавливается в Login.continueLoading */
    private _stateWrapper?: FunctionComponentStateWrapper<T>;
    public get stateWrapper(): FunctionComponentStateWrapper<T>|undefined {return this._stateWrapper};// возможно, undefined стоит убрать
    public set stateWrapper(value: FunctionComponentStateWrapper<T>) {this._stateWrapper = value};


    /** Восстановление начальных значений */
    public init = () => {
        this._commonState = 0;
        this._commonString = "";
    }
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
