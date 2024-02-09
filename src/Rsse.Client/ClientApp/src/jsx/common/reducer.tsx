import {connect} from "react-redux";
import {CatalogContainer} from "../components/catalog.redux.tsx";

// Рефакторинг под react-redux: результаты:
// I. Catalog: получилось избавиться от стейт-машины, redux не понадобился, остался recovery context: разделение на container - view
// II. Read: убрана стейт-машина, common context используется в Note для сохранения id последней отображенной заметки
// III. Create:
// IV. Login:
// V. Update:
// VI. Menu: 

// REACT-REDUX: демонстрационный код для react-redux:

// CONSTANTS:
export const GET_COMMON_STATE: string = "get_common_state";
export const SET_COMMON_STATE: string = "set_common_state";

// ACTIONS:
export const getCommonState = (): {type:string} => ({
    type: GET_COMMON_STATE
});
export const setCommonState = (value:number): {type:string,value:number} => ({
    type: SET_COMMON_STATE,
    value: value
});

// REDUCER:
export const reducer = (state: {commonState: number}, action: any): /*{commonState: number}*/any => {
    if (!state) {
        return {commonState: 0};
    }

    switch (action.type){
        case GET_COMMON_STATE:
            return {
                commonState: state.commonState// - 1
            };
        case SET_COMMON_STATE:
            // как поменять значение в хранилище без ре-рендеринга?
            // state.commonState = action.value;
            // return state;
            return {
               commonState: action.value
            };
        default:
            return state;
    }
}

// две функции формируют набор props для вызова компонента:
// I. обновления хранилища передаётся компоненту в виде свойств
// подписка и ре-рендер при новых данных: вопрос - как при необходимости не рендерить заново компонент?
const mapStateToProps = (state: any) => ({
    payload: state.commonState
});
// II. доступ к действиям передаётся компоненту в виде свойств:
const mapDispatchToProps = (dispatch: any) => ({
    onSet(value: number): void {
        // const number = Number(value);
        dispatch(setCommonState(value))
    },
    onGet(): void {
        dispatch(getCommonState())
    }
});

// PRESENTATIONAL COMPONENT: компонент, находящийся в контексте провайдера:
export const PresentationalSimpleComponent = (props: {onGet:()=>void, onSet(v: string):void, payload:string}) => {
    return (
        <div>
            <button onClick={e => props.onSet(e.currentTarget.value)} value={10}>=</button>
            <button onClick={props.onGet}>-</button>
            <span>{props.payload}</span>
        </div>
    );
}

// CONTAINER COMPONENT: подключаем презентационный компонент к хранилищу redux:
export const CatalogContainerComponent = connect(
    mapStateToProps,
    mapDispatchToProps
)(CatalogContainer);
// /*props: {onGet:()=>void, onSet(v: number):void, payload:string}*/
