import {ReadView} from "./read.component";
import {UpdateView} from "./update.component";
import {CreateView} from "./create.component";
import {CatalogView} from "./catalog.component";
import {LoginComponent} from "./login.component";

import {
    HashRouter,
    NavLink,
    Routes,
    Route
} from 'react-router-dom';
import {CommonStateStorage} from "../common/state.wrappers";
import {CatalogResponseDto, NoteResponseDto} from "../dto/request.response.dto";
import {CommonContextProvider} from "../common/context.provider";

export const App = () => {
    const commonStateStorage = new CommonStateStorage<NoteResponseDto|CatalogResponseDto>();
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
                    <CommonContextProvider value={commonStateStorage}>
                        <Routes>
                            <Route path="/" element={<ReadView/>}/>
                            <Route path="/read/:textId" element={<ReadView/>}/>
                            <Route path="/update" element={<UpdateView/>}/>
                            <Route path="/create" element={<CreateView/>}/>
                            <Route path="/catalog" element={<CatalogView/>}/>
                        </Routes>
                        <LoginComponent />
                    </CommonContextProvider>
                </div>
            </div>
        </HashRouter>);
}
