import { loadConfiguration, TestConfig } from '../config/configuration-loader';
import { ScenarioDsl, AppContext, ChannelMode } from './dsl/scenario-dsl';
import { ShopApiDriver } from './drivers/shop-api-driver';
import { ShopUiDriver } from './drivers/shop-ui-driver';
import { ErpRealDriver } from './drivers/erp-real-driver';
import { ErpStubDriver } from './drivers/erp-stub-driver';
import { ClockRealDriver } from './drivers/clock-real-driver';
import { ClockStubDriver } from './drivers/clock-stub-driver';
import { ErpDriver, ClockDriver } from './drivers/types';
import { Browser } from 'playwright';

export type Channel = 'api' | 'ui';
export { ChannelMode } from './dsl/scenario-dsl';
export type ExternalSystemMode = 'real' | 'stub';

export interface ScenarioOptions {
  channel?: Channel;
  channelMode?: ChannelMode;
  externalSystemMode?: ExternalSystemMode;
  browser?: Browser;
}

export function createScenario(options: ScenarioOptions = {}): ScenarioDsl {
  const mode = options.externalSystemMode || 'real';
  const config = loadConfiguration({ externalSystemMode: mode });

  const channelMode: ChannelMode = options.channelMode || (process.env.CHANNEL_MODE?.toLowerCase() as ChannelMode) || 'dynamic';
  const channel = options.channel || 'api';

  const app = new AppContext({
    channelMode,
    channel,
    shopDriverFactory: (ch) => createShopDriverForChannel(config, ch as Channel, options),
    erpDriver: createErpDriver(config, mode),
    clockDriver: createClockDriver(config, mode),
  });

  return new ScenarioDsl(app);
}

function createShopDriverForChannel(config: TestConfig, channel: Channel, options: ScenarioOptions) {
  if (channel === 'ui') {
    if (!options.browser) throw new Error('Browser is required for UI channel');
    return new ShopUiDriver(config.shop.frontendUrl, options.browser);
  }
  return new ShopApiDriver(config.shop.backendApiUrl);
}

function createErpDriver(config: TestConfig, mode: ExternalSystemMode): ErpDriver {
  if (mode === 'stub') return new ErpStubDriver(config.externalSystems.erp.url);
  return new ErpRealDriver(config.externalSystems.erp.url);
}

function createClockDriver(config: TestConfig, mode: ExternalSystemMode): ClockDriver {
  if (mode === 'stub') return new ClockStubDriver(config.externalSystems.clock.url);
  return new ClockRealDriver();
}
