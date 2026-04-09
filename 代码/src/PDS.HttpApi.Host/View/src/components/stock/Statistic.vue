<template>
  <a-layout class="hb-layout">
    <a-layout-header class="content-layout-header">
      <a-descriptions :column="4">
        <a-descriptions-item :label="$t('inputTitle.skuCode')">
          <a-input v-model="query.sku"></a-input>
        </a-descriptions-item>
        <a-descriptions-item :label="$t('inputTitle.wareHouseId')">
          <a-input v-model="query.wareHouseId"></a-input>
        </a-descriptions-item>
        <a-descriptions-item>
          <a-button type="primary" icon="search" size="small" @click="load">
            {{ $t("searchButton") }}
          </a-button>
        </a-descriptions-item>
      </a-descriptions>
    </a-layout-header>
    <a-layout-content class="content-layout-content">
      <a-table
        :columns="columns"
        :data-source="data"
        bordered
        :rowKey="(row, index) => row.id"
        size="small"
        :pagination="pagination"
        :loading="loading"
        @change="change"
      >
        <!-- <template slot="operation" slot-scope="text, record">
          <a
            href="javascript:void(0);"
            @click="show('edit', record)"
            v-if="isGranted('AbpIdentity.Users.Update')"
          >
            <a-icon type="edit" />{{ $t("actions.edit") }}
          </a>
          <a-popconfirm
            :title="$t('confirm.delete')"
            @confirm="remove(record)"
            v-if="isGranted('AbpIdentity.Users.Delete')"
          >
            <a href="javascript:;">
              <a-icon type="delete" />{{ $t("actions.delete") }}
            </a>
          </a-popconfirm>
        </template> -->
      </a-table>
    </a-layout-content>
  </a-layout>
</template>
<script>
import { Statistic } from "@/api/stock/stock";
import locale from "@/localize/materials/localize";
import component from "@/lib/base";
export default {
  name: "Store",
  mixins: [component],
  i18n: {
    messages: locale,
  },
  data() {
    return {
      loading: false,
      showForm: false,
      pagination: {
        current: 1,
        pageSize: 10,
        total: 0,
        "show-total": (total) => `共${total}条数据`,
        "show-size-changer": true,
      },
      columns: [
        {
          dataIndex: "wareHouseId",
          title: this.$t("tableTitle.wareHouseId"),
          align: "center",
        },
        {
          dataIndex: "sku",
          title: this.$t("tableTitle.sku"),
          align: "center",
        },
        {
          dataIndex: "materialName",
          title: this.$t("tableTitle.name"),
          align: "center",
        },
        {
          dataIndex: "sumQuatity",
          title: this.$t("tableTitle.totalQuantity"),
          align: "center",
        },
        {
          dataIndex: "availableQuatity",
          title: this.$t("tableTitle.availableQuantity"),
          align: "center",
        },
        {
          dataIndex: "lockedQuatity",
          title: this.$t("tableTitle.lockedQuantity"),
          align: "center",
        },
        {
          dataIndex: "freezeQuatity",
          title: this.$t("tableTitle.freezeQuantity"),
          align: "center",
          // scopedSlots: { customRender: "operation" },
        },
      ],
      data: [],
      query: {
        sku: "",
        wareHouseId: "",
      },
    };
  },
  mounted() {
    this.load();
  },
  methods: {
    load: async function () {
      let _this = this;
      try {
        _this.loading = true;
        let query = Object.assign(
          {
            skipCount: (this.pagination.current - 1) * this.pagination.pageSize,
            maxResultCount: this.pagination.pageSize,
          },
          this.query
        );
        let result = await Statistic(query);
        _this.data = result.items;
        _this.pagination.total = result.totalCount;
      } catch (err) {
        this.$message.error(err.message);
      } finally {
        this.loading = false;
      }
    },
    change: function (pagination) {
      this.pagination = pagination;
      this.load();
    },
  },
};
</script>
