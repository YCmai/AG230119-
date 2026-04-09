<template>
  <a-layout class="hb-layout">
    <a-layout-header class="content-layout-header">
      <a-descriptions style="text-align: left" :column="4">
        <a-descriptions-item label="库别编码">
          <a-input v-model="query.wareHouseId"></a-input>
        </a-descriptions-item>
        <a-descriptions-item>
          <a-button type="primary" icon="search" size="small" @click="load">
            搜索
          </a-button>
        </a-descriptions-item>
      </a-descriptions>
    </a-layout-header>
    <a-layout-content class="content-layout-content">
      <a-row class="layout-content-operation">
        <a-col :span="24">
          <a-button icon="plus" size="small" type="primary" @click="showWare()">
            {{ $t("actions.add") }}
          </a-button>
        </a-col>
      </a-row>
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
        <area-list-vue
          slot="expandedRowRender"
          slot-scope="ware"
          :wareHouse="ware"
          @load="load"
        />
        <template slot="operation" slot-scope="text, record">
          <a-space>
            <a href="javascript:void(0);" @click="showArea(record)">
              <a-icon type="plus-circle" />新增区域
            </a>
            <a href="javascript:void(0);" @click="showWare(record.id)">
              <a-icon type="edit" />{{ $t("actions.edit") }}
            </a>
            <a href="javascript:void(0);" @click="deleteWare(record.id)">
              <a-icon type="edit" />{{ $t("actions.delete") }}
            </a>
          </a-space>
        </template>
      </a-table>
      <form-vue v-if="showWareForm" @close="closeWare" ref="wareForm"></form-vue>
      <area-form-vue
        v-if="showAreaForm"
        @close="closeArea"
        ref="areaForm"
      ></area-form-vue>
    </a-layout-content>
  </a-layout>
</template>
<script>
import locale from "@/localize/warehouse/warehouse";
import FormVue from "./Form.vue";
import AreaListVue from "./area/List.vue";
import { GetWareHouses as GetAll, DeleteWareHouse } from "@/api/inventory/wareHouse";
import component from "@/lib/base";
import AreaFormVue from "./area/Form.vue";
export default {
  name: "WareHouseList",
  components: { FormVue, AreaFormVue, AreaListVue },
  mixins: [component],
  i18n: {
    messages: locale,
  },
  data() {
    return {
      loading: false,
      pagination: {
        total: 0,
        current: 1,
        pageSize: 10,
        "show-total": (total) => `共${total}条数据`,
        "show-size-changer": true,
      },
      data: [],
      columns: [
        {
          dataIndex: "id",
          title: this.$t("columns.code"),
          width: 150,
        },
        {
          dataIndex: "name",
          title: this.$t("columns.name"),
          width: 150,
        },

        {
          dataIndex: "description",
          title: this.$t("columns.description"),
        },
        {
          dataIndex: "operations",
          title: this.$t("title.actions"),
          align: "center",
          scopedSlots: { customRender: "operation" },
          width: 200,
        },
      ],
      showAreaForm: false,
      showWareForm: false,
      query: { wareHouseId: "" },
    };
  },
  mounted() {
    this.load();
  },
  methods: {
    load: async function () {
      this.loading = true;
      try {
        let query = Object.assign(this.query, {
          skipCount: (this.pagination.current - 1) * this.pagination.pageSize,
          maxResultCount: this.pagination.pageSize,
        });
        let result = await GetAll(query);
        this.data = result.items;
        this.pagination.total = result.totalCount;
      } catch (err) {
        this.$message.error(err.messages);
      } finally {
        this.loading = false;
      }
    },
    change: function (pagination) {
      this.pagination = pagination;
    },
    showWare: function (id) {
      this.showWareForm = true;
      if (id) {
        this.$nextTick(() => {
          this.$refs.wareForm.load(id);
        });
      }
    },
    closeWare: function () {
      this.showWareForm = false;
      this.load();
    },
    deleteWare: async function (id) {
      if (!(await this.confirm("确定删除数据?"))) return;
      let spin = this.$spin({ text: "提交数据中...", target: ".content-layout-content" });
      try {
        await DeleteWareHouse(id);
        this.load();
      } catch (err) {
        this.$message.error(err.message);
      } finally {
        spin.close();
      }
    },
    showArea: function (wareHouse = {}) {
      this.showAreaForm = true;
      this.$nextTick(() => {
        this.$refs.areaForm.load("", wareHouse);
      });
    },
    closeArea: function () {
      this.showAreaForm = false;
      this.load();
    },
  },
};
</script>
<style scoped>
.ant-table-tbody tr:nth-child(2n) {
  background-color: black !important;
}
</style>
