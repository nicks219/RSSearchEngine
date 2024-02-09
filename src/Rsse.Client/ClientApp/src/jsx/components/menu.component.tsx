import {ReadContainer} from "./read.container";
import {UpdateContainer} from "./update.container";
import {CreateContainer} from "./create.container";
import {LoginContainer} from "./login.container";
import {CatalogContainer} from "./catalog.container";

import {HashRouter, NavLink, Routes, Route} from 'react-router-dom';
import {CommonStateStorage, RecoveryStateStorage, StateTypesAlias} from "../common/state.handlers";
import {CommonContextProvider, RecoveryContextProvider} from "../common/context.provider";

import {createStore} from "redux";
import {Provider} from "react-redux";
import {reducer} from "../common/reducer";

export const App = () => {
    const commonStateStorage = new CommonStateStorage();
    const recoveryStateStorage = new RecoveryStateStorage<StateTypesAlias>();

    // redux sample:
    const initialState = {commonState: 0};
    const reduxStore = createStore(reducer, initialState);
    reduxStore.subscribe(() => {
        const value = reduxStore.getState();
        console.log(`Redux store log: common state: ${value.commonState}`)
    });
    // reduxStore.dispatch(setCommonState(0));

    return (
        <HashRouter>
            <div>
                <div id="header">
                    <ul>
                        <li><NavLink to="/">Посмотреть</NavLink></li>
                        <li><NavLink to="/update">Поменять</NavLink></li>
                        <li><NavLink to="/create">Создать</NavLink></li>
                        <li><NavLink to="/catalog">Каталог</NavLink></li>
                    </ul>
                </div>

                <div id="renderContainer1">
                    <Provider store={reduxStore}>{/* провайдер react-redux */}
                        <RecoveryContextProvider value={recoveryStateStorage}>{/* провайдер recovery */}
                            <CommonContextProvider value={commonStateStorage}>
                                <Routes>
                                    <Route path="/" element={<ReadContainer/>}/>
                                    <Route path="/read/:textId" element={<ReadContainer/>}/>
                                    <Route path="/update" element={<UpdateContainer/>}/>
                                    <Route path="/create" element={<CreateContainer/>}/>
                                    <Route path="/catalog" element={<CatalogContainer/>}/>
                                </Routes>
                                <LoginContainer/>
                            </CommonContextProvider>
                        </RecoveryContextProvider>
                    </Provider>
                </div>
            </div>
        </HashRouter>);
}
