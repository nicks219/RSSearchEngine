import {describe, it, expect} from 'vitest';
import {render, screen} from '@testing-library/react';
import {App} from "../components/menu.component";

describe('MenuRouter component tests', () => {
    it('menu elements should render', () => {
        // tutorial variant:
        // const { getAllByText } = render(<MenuRouter />);
        // expect(
        //     getAllByText('Посмотреть')[0]
        // ).toBeInTheDocument();

        // arrange:
        render(<App />);
        // act:
        const firstMenuElement = screen.getByText("Посмотреть");
        const secondMenuElement = screen.getByText("Поменять");
        const thirdMenuElement = screen.getByText("Создать");
        const forthMenuElement = screen.getByText("Каталог");
        // assert:
        expect(firstMenuElement).toBeInTheDocument();
        expect(secondMenuElement).toBeInTheDocument();
        expect(thirdMenuElement).toBeInTheDocument();
        expect(forthMenuElement).toBeInTheDocument();
    });
})
