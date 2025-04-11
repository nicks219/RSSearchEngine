import {ReadContainer} from "./read.container";
import {UpdateContainer} from "./update.container";
import {CreateContainer} from "./create.container";
import {LoginContainer} from "./login.container";
import {CatalogContainer} from "./catalog.container";

import {HashRouter, NavLink, Routes, Route} from 'react-router-dom';
import {CommonStateStorage, RecoveryStateStorage, StateTypesAlias} from "../common/state.handlers";
import {CommonContextProvider, RecoveryContextProvider} from "../common/context.provider";
import {Doms, SystemConstants} from "../dto/doms.tsx";

export const App = () => {
    const commonStateStorage = new CommonStateStorage();
    const recoveryStateStorage = new RecoveryStateStorage<StateTypesAlias>();

    return (
        <HashRouter>
            <div>
                <div id={Doms.header}>
                    <ul>
                        <li><NavLink to={SystemConstants.emptySegment}>Посмотреть</NavLink></li>
                        <li><NavLink to={SystemConstants.updatePath}>Поменять</NavLink></li>
                        <li><NavLink to={SystemConstants.createPath}>Создать</NavLink></li>
                        <li><NavLink to={SystemConstants.catalogPath}>Каталог</NavLink></li>
                    </ul>
                </div>

                <div id={Doms.renderContainer1}>
                    <RecoveryContextProvider value={recoveryStateStorage}>{/* провайдер recovery */}
                        <CommonContextProvider value={commonStateStorage}>
                            <Routes>
                                <Route path={SystemConstants.emptySegment} element={<ReadContainer/>}/>
                                <Route path="/read/:textId" element={<ReadContainer/>}/>
                                <Route path={SystemConstants.updatePath} element={<UpdateContainer/>}/>
                                <Route path={SystemConstants.createPath} element={<CreateContainer/>}/>
                                <Route path={SystemConstants.catalogPath} element={<CatalogContainer/>}/>
                            </Routes>
                        </CommonContextProvider>
                    </RecoveryContextProvider>
                </div>
                
                <div id={Doms.footer}>
                    <LoginContainer/>
                </div>
            </div>
        </HashRouter>);
}
