import type { Meta, StoryObj } from '@storybook/react';
import { KcPage } from "../kc.gen";
import { getKcContextMock } from "./mocks/getKcContextMock";

const meta = {
  title: 'login/KcPage',
  component: KcPage,
} satisfies Meta<typeof KcPage>;

export default meta;

type Story = StoryObj<typeof meta>;

export const Default: Story = {
  args: {
    kcContext: getKcContextMock({
      pageId: "login.ftl",
      overrides: {}
    })
  }
};
